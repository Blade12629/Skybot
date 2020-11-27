using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using SkyBot.Database.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;

namespace SkyBot.Discord.CommandSystem
{
    /// <summary>
    /// Command handler for handling discord commands
    /// </summary>
    public sealed class CommandHandler : IDisposable
    {
        public DiscordHandler DiscordHandler { get; private set; }
        public bool IsDisposed { get; private set; }
        public char CommandPrefix { get; private set; }
        public Action<DiscordChannel, ICommand, string> OnException { get; set; }

        public IReadOnlyDictionary<string, ICommand> Commands => _commandTypes;

        private ConcurrentDictionary<string, ICommand> _commandTypes;
        private CommandAssemblyLoadContext _commandAssemblyLoadContext;

        public CommandHandler(DiscordHandler discordHandler, char commandPrefix)
        {
            DiscordHandler = discordHandler;
            CommandPrefix = commandPrefix;
        }

        ~CommandHandler()
        {
            Dispose();
        }

        /// <summary>
        /// Loads commands from an assembly without locking the assembly
        /// </summary>
        /// <param name="commandAssemblyFile">Filepath</param>
        /// <returns>Commands loaded</returns>
        public bool LoadCommands(string commandAssemblyFile)
        {
            _commandAssemblyLoadContext = new CommandAssemblyLoadContext();
            _commandTypes = new ConcurrentDictionary<string, ICommand>();

            if (!_commandAssemblyLoadContext.Load(commandAssemblyFile))
                return false;

            foreach(Type t in _commandAssemblyLoadContext.Assembly.GetTypes())
            {
                if (t.GetInterface(nameof(ICommand)) == null)
                    continue;

                RegisterCommand(t);
            }

            return true;
        }

        public bool UnloadCommands()
        {
            if (_commandAssemblyLoadContext == null)
                return true;

            _commandTypes.Clear();
            _commandTypes = null;

            GC.Collect(); //lgtm [cs/call-to-gc]

            bool result = _commandAssemblyLoadContext.Unload();
            _commandAssemblyLoadContext = null;

            return result;
        }

        public bool ReloadCommands(string commandAssemblyFile)
        {
            if (!UnloadCommands())
                return false;

            return LoadCommands(commandAssemblyFile);
        }

        /// <summary>
        /// Registers a <see cref="ICommand"/>
        /// </summary>
        /// <param name="commandType">Type must derive from <see cref="ICommand"/></param>
        /// <returns>Successfull registered</returns>
        public bool RegisterCommand(Type commandType)
        {
            if (!(Activator.CreateInstance(commandType) is ICommand cmd) || 
                !_commandTypes.TryAdd(cmd.Command.ToLower(System.Globalization.CultureInfo.CurrentCulture), cmd))
                return false;

            Logger.Log($"Registered command {cmd.Command}");

            return true;
        }

        /// <summary>
        /// Deregisters a <see cref="ICommand"/>
        /// </summary>
        /// <param name="command">command name</param>
        public void DeRegisterCommand(string command)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException(nameof(command));

            command = command.TrimStart(CommandPrefix).ToLower(System.Globalization.CultureInfo.CurrentCulture);

            _commandTypes.TryRemove(command, out ICommand _);

            Logger.Log($"Registered command {command}");
        }

        /// <summary>
        /// Disposes the <see cref="CommandHandler"/>
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed)
                return;

            DiscordHandler = null;

            GC.SuppressFinalize(this);

            IsDisposed = true;
        }

        /// <summary>
        /// Invokes a <see cref="ICommand"/>
        /// </summary>
        public void Invoke(MessageCreateEventArgs e)
        {
            try
            {
                if (e == null)
                    return;

                DiscordGuildConfig config = null;
                char guildPrefix = CommandPrefix;
                if (e.Guild != null)
                {
                    using DBContext c = new DBContext();
                    config = c.DiscordGuildConfig.FirstOrDefault(dgc => dgc.GuildId == (long)e.Guild.Id);
                    User user = c.User.FirstOrDefault(u => u.DiscordUserId == (long)e.Author.Id);
                    long osuId = user == null ? 0 : user.OsuUserId;

                    List<BannedUser> bans = BanManager.GetBansForUser((long)e.Author.Id, osuId, e.Guild == null ? 0 : (long)e.Guild.Id);

                    if (bans.Count > 0)
                    {
                        OnBannedUserDetected(e, bans);
                        return;
                    }

                    if (config != null && config.Prefix.HasValue)
                        guildPrefix = config.Prefix.Value;
                }

                if (!e.Message.Content[0].Equals(guildPrefix))
                    return;

                List<string> parameters = e.Message.Content.Split(' ').Skip(0).ToList();

                if (parameters == null)
                    parameters = new List<string>();

                string command;
                if (parameters.Count == 0)
                    command = e.Message.Content;
                else
                    command = parameters[0];

                command = command.TrimStart(guildPrefix);

                AccessLevel access = GetAccessLevel(e.Author.Id, e.Guild?.Id ?? 0);

                if (!_commandTypes.TryGetValue(command.ToLower(System.Globalization.CultureInfo.CurrentCulture), out ICommand cmd) ||
                    config != null && access <= AccessLevel.VIP && config.CommandChannelId > 0 && config.CommandChannelId != (long)e.Channel.Id)
                    return;
                else if (cmd.IsDisabled)
                {
                    e.Channel.SendMessageAsync("Command is currently disabled");
                    return;
                }

                switch (cmd.CommandType)
                {
                    default:
                    case CommandType.None:
                        break;

                    case CommandType.Private:
                        if (e.Guild != null)
                        {
                            e.Channel.SendMessageAsync("You can only use this command in a private chat!");
                            return;
                        }
                        break;

                    case CommandType.Public:
                        if (e.Guild == null)
                        {
                            e.Channel.SendMessageAsync("You can only use this command in a server chat!");
                            return;
                        }
                        break;
                }

                AccessLevel cmdAccess = e.Guild == null ? cmd.AccessLevel : GetCommandAccessLevel(cmd, e.Guild.Id);

                if (access < cmdAccess)
                {
                    OnException(e.Channel, cmd, "You do not have enough permissions to use this command");
                    return;
                }

                if (parameters.Count > 0)
                    parameters.RemoveAt(0);

                if (cmd.MinParameters > 0 && parameters.Count < cmd.MinParameters)
                {
                    OnException(e.Channel, cmd, "Not enough parameters");
                    return;
                }

                string afterCmd = e.Message.Content;

                if (afterCmd.Length > cmd.Command.Length + 1)
                    afterCmd = afterCmd.Remove(0, cmd.Command.Length + 2);
                else
                    afterCmd = string.Empty;

                DiscordMember member = null;
                if (e.Guild != null)
                {
                    try
                    {
                        member = e.Guild.GetMemberAsync(e.Author.Id).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                    catch (AggregateException ex)
                    {
                        if (!ex.InnerExceptions.Any(e => e is NotFoundException))
                            throw;
                    }
                }

                CommandEventArg arg = new CommandEventArg(e.Guild, e.Channel, e.Author, member,
                                                          e.Message, access, parameters, afterCmd, 
                                                          config);

                ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
                {
                    try
                    {
                        cmd.Invoke(DiscordHandler, this, arg);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        if (ex is UnauthorizedException)
                        {
                            OnException?.Invoke(e.Channel, cmd, "Unauthorized, please allow direct messages (if you have direct messages enabled and this keeps happening, please report it)");
                            Logger.Log($"Unauthorized: " + ex, LogLevel.Warning);

                            return;
                        }

                        string debugMsg = $"Something went wrong while invoking command {cmd.Command}, message: {arg.Message.Content} from {arg.User.Username}#{arg.User.Discriminator} ({arg.User.Id}):\n {ex.ToString()}";
                        Logger.Log(debugMsg, LogLevel.Warning);

                        if (arg.Config != null && arg.Config.Debug)
                        {
                            OnException?.Invoke(e.Channel, cmd, GetDebugExceptionMessage(ex));

                            if (arg.Config.DebugChannel != 0)
                            {
                                try
                                {
                                    var dchannel = arg.Guild.GetChannel((ulong)arg.Config.DebugChannel);
                                    dchannel.SendMessageAsync(debugMsg).ConfigureAwait(false);
                                }
#pragma warning disable CA1031 // Do not catch general exception types
                                catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
                                {
                                    //If we catch an exception here, there is nothing we can do, so just ignore it
                                }
                            }
                        }
                        else
                            OnException?.Invoke(e.Channel, cmd, $"Something went wrong executing this command");
                    }
                }));

            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Logger.Log(ex, LogLevel.Error); 
            }

            string GetDebugExceptionMessage(Exception ex)
            {
                return $"Something went wrong executing this command (L: {ex.GetLineNumber()} At: {ex.TargetSite.DeclaringType}.{ex.TargetSite.Name}: {ex.Message})";
            }
        }

        private static void OnBannedUserDetected(MessageCreateEventArgs e, List<BannedUser> bans)
        {
            //placeholder
            return;
        }

        /// <summary>
        /// Gets the accesslevel for a command, this also checks if the accesslevel is overriden
        /// </summary>
        /// <returns>Default or overwritten access level</returns>
        public static AccessLevel GetCommandAccessLevel(ICommand cmd, ulong discordGuildId)
        {
            if (cmd == null)
                throw new ArgumentNullException(nameof(cmd));

            int access = (int)cmd.AccessLevel;

            using DBContext c = new DBContext();
            CommandAccess cmdAccess = null;

            if (discordGuildId > 0)
                cmdAccess = c.CommandAccess.FirstOrDefault(ca => ca.DiscordGuildId == (long)discordGuildId &&
                                                           ca.TypeName.Equals(cmd.GetType().Name, StringComparison.CurrentCultureIgnoreCase));

            if (cmdAccess != null && cmdAccess.AccessLevel != access)
                access = cmdAccess.AccessLevel;

            return (AccessLevel)access;
        }

        /// <summary>
        /// Sets the <see cref="AccessLevel"/> of a command for a specific guild
        /// </summary>
        /// <returns>True - changed, False - cannot be overwritten, command not found or command/requested access is dev</returns>
        public static bool SetCommandAccessLevel(ICommand cmd, ulong discordGuildId, AccessLevel newAccess)
        {
            if (cmd == null)
                throw new ArgumentNullException(nameof(cmd));
            else if (!cmd.AllowOverwritingAccessLevel ||
                     cmd.AccessLevel == AccessLevel.Dev ||
                     newAccess == AccessLevel.Dev)
                return false;

            using DBContext c = new DBContext();
            CommandAccess cmdAccess = c.CommandAccess.FirstOrDefault(ca => ca.DiscordGuildId == (long)discordGuildId &&
                                                                           ca.TypeName.Equals(cmd.GetType().Name, StringComparison.CurrentCultureIgnoreCase));

            if (cmdAccess == null)
            {
                cmdAccess = new CommandAccess((long)discordGuildId, cmd.GetType().Name, (int)newAccess);
                c.CommandAccess.Add(cmdAccess);
            }
            else
            {
                int access = (int)newAccess;

                if (cmdAccess.AccessLevel != access)
                    cmdAccess.AccessLevel = access;

                c.CommandAccess.Update(cmdAccess);
            }

            c.SaveChanges();
            return true;
        }

        /// <summary>
        /// Sets the <see cref="AccessLevel"/> of a command for a specific guild
        /// </summary>
        /// <returns>True - changed, False - cannot be overwritten, command not found or command/requested access is dev</returns>
        public bool SetCommandAccessLevel(string command, ulong discordGuildId, AccessLevel newAccess)
        {
            if (string.IsNullOrEmpty(command))
                throw new ArgumentNullException(nameof(command));
            
            if (!Commands.TryGetValue(command.ToLower(CultureInfo.CurrentCulture), out ICommand cmd))
                return false;

            return SetCommandAccessLevel(cmd, discordGuildId, newAccess);
        }

        /// <summary>
        /// Gets the access level of the user, either directly or via roles
        /// </summary>
        public AccessLevel GetAccessLevel(ulong discordUserId, ulong discordGuildId)
        {
            using DBContext c = new DBContext();

            List<Permission> perms = c.Permission.Where(p => p.DiscordUserId == (long)discordUserId).ToList();

            //Check if we are dev
            for (int i = 1; i < perms.Count; i++)
                if (perms[i].AccessLevel == (short)AccessLevel.Dev)
                    return AccessLevel.Dev;

            //default access level
            short access = (short)AccessLevel.User;

            if (discordGuildId != 0)
            {
                try
                {
                    DiscordGuild guild = DiscordHandler.GetGuildAsync(discordGuildId).Result;

                    //Check if user is owner
                    if (guild.Owner.Id == discordUserId)
                        return AccessLevel.Host;

                    //check if any permission is higher than our old permission but only those that are for our guild
                    perms = perms.Where(p => p.DiscordGuildId == (long)guild.Id).ToList();

                    for (int i = 0; i < perms.Count; i++)
                        if (perms[i].AccessLevel > access)
                            access = perms[i].AccessLevel;

                    DiscordMember member = guild.GetMemberAsync(discordUserId).Result;

                    //Check if we have any roles binded
                    List<DiscordRole> roles = member.Roles.ToList();

                    for (int i = 0; i < roles.Count; i++)
                    {
                        DiscordRoleBind drb = c.DiscordRoleBind.FirstOrDefault(drb => drb.RoleId == (long)roles[i].Id);

                        if (drb == null)
                            continue;

                        if (access < drb.AccessLevel)
                            access = drb.AccessLevel;
                    }
                }
                catch (AggregateException ex)
                {
                    if (!ex.InnerExceptions.Any(e => e is NotFoundException))
                        throw;
                }
            }

            return (AccessLevel)access;
        }

        /// <summary>
        /// Sets the accesslevel of a user
        /// </summary>
        /// <param name="newAccess">New <see cref="AccessLevel"/></param>
        public static void SetAccessLevel(ulong discordUserId, ulong discordGuildId, AccessLevel newAccess)
        {
            using DBContext c = new DBContext();
            Permission perm = c.Permission.FirstOrDefault(p => p.DiscordUserId == (long)discordUserId &&
                                                               p.DiscordGuildId == (long)discordGuildId);

            if (perm == null)
            {
                perm = new Permission((long)discordUserId, (long)discordGuildId, newAccess);
                c.Permission.Add(perm);
            }
            else
            {
                perm.AccessLevel = (short)newAccess;
                c.Permission.Update(perm);
            }

            c.SaveChanges();
        }

        /// <summary>
        /// Sets the accesslevel of a user
        /// </summary>
        /// <param name="newAccess">New <see cref="AccessLevel"/></param>
        public static void SetAccessLevel(DiscordUser user, DiscordGuild guild, AccessLevel newAccess)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            else if (guild == null)
                throw new ArgumentNullException(nameof(guild));

            SetAccessLevel(user.Id, guild.Id, newAccess);
        }

        /// <summary>
        /// Gets the access level of the user, either directly or via roles
        /// </summary>
        public AccessLevel GetAccessLevel(DiscordUser user, DiscordGuild guild)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));
            else if (guild == null)
                throw new ArgumentNullException(nameof(guild));

            return GetAccessLevel(user.Id, guild?.Id ?? 0);
        }

        /// <summary>
        /// Binds an <see cref="AccessLevel"/> to a <see cref="DiscordRole"/>
        /// </summary>
        /// <returns></returns>
        public static bool BindPermssion(DiscordGuild guild, ulong roleId, AccessLevel access)
        {
            if (guild == null)
                throw new ArgumentNullException(nameof(guild));
            else if (roleId == 0)
                throw new ArgumentOutOfRangeException(nameof(roleId));

            using DBContext c = new DBContext();
            DiscordRoleBind drb = c.DiscordRoleBind.FirstOrDefault(drb => drb.GuildId == (long)guild.Id &&
                                                                          drb.RoleId == (long)roleId &&
                                                                          drb.AccessLevel == (short)access);

            if (drb != null)
                return true;

            drb = new DiscordRoleBind((long)guild.Id, (long)roleId, (short)access);

            c.DiscordRoleBind.Add(drb);
            c.SaveChanges();

            return true;
        }

        /// <summary>
        /// Unbinds an/all <see cref="AccessLevel"/> from a <see cref="DiscordRole"/>
        /// </summary>
        /// <param name="access">Leave empty to unbind all permissions from a role</param>
        /// <returns></returns>
        public static bool UnbindPermission(DiscordGuild guild, ulong roleId, AccessLevel? access = null)
        {
            if (guild == null)
                throw new ArgumentNullException(nameof(guild));
            else if (roleId == 0)
                throw new ArgumentOutOfRangeException(nameof(roleId));

            using DBContext c = new DBContext();
            List<DiscordRoleBind> drb = c.DiscordRoleBind.Where(drb => drb.GuildId == (long)guild.Id &&
                                                                       drb.RoleId == (long)roleId).ToList();

            if (drb.Count == 0)
                return false;

            if (access.HasValue)
                drb = drb.Where(d => d.AccessLevel == (short)access.Value).ToList(); //lgtm [cs/dereferenced-value-may-be-null]

            if (drb.Count == 0)
                return false;

            c.DiscordRoleBind.RemoveRange(drb);

            c.SaveChanges();
            return true;
        }
    }
}
