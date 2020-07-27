using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using SkyBot.Database.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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

            GC.Collect();

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
            ICommand cmd = Activator.CreateInstance(commandType) as ICommand;

            if (cmd == null || !_commandTypes.TryAdd(cmd.Command.ToLower(System.Globalization.CultureInfo.CurrentCulture), cmd))
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

            GC.Collect();
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
                if (e == null || !e.Message.Content[0].Equals(CommandPrefix))
                    return;

                List<string> parameters = e.Message.Content.Split(' ').Skip(0).ToList();

                if (parameters == null)
                    parameters = new List<string>();

                string command;
                if (parameters.Count == 0)
                    command = e.Message.Content;
                else
                    command = parameters[0];

                command = command.TrimStart(CommandPrefix);

                if (!_commandTypes.TryGetValue(command.ToLower(System.Globalization.CultureInfo.CurrentCulture), out ICommand cmd))
                    return;
                else if (cmd.IsDisabled)
                {
                    e.Channel.SendMessageAsync("Command is currently disabled");
                    return;
                }

                switch(cmd.CommandType)
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

                AccessLevel access = GetAccessLevel(e.Author.Id, e.Guild?.Id ?? 0);

                if (access < cmd.AccessLevel)
                    return;

                if (parameters.Count > 0)
                    parameters.RemoveAt(0);

                CommandEventArg arg = new CommandEventArg(e.Guild, e.Channel, e.Author, (e.Guild == null ? null : e.Guild.GetMemberAsync(e.Author.Id).Result),
                                                          e.Message, access, parameters);

                ThreadPool.QueueUserWorkItem(new WaitCallback(o =>
                {
                    try
                    {
                        cmd.Invoke(this, arg);
                    }
#pragma warning disable CA1031 // Do not catch general exception types
                    catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                    {
                        Logger.Log($"Something went wrong while invoking command {cmd.Command}: {ex.ToString()}");
                        OnException?.Invoke(e.Channel, cmd, "Something went wrong executing this command");
                    }
                }));

            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Logger.Log(ex, LogLevel.Error); 
            }
        }

        /// <summary>
        /// Gets the access level of the user, either directly or via roles
        /// </summary>
        public AccessLevel GetAccessLevel(ulong discordUserId, ulong discordGuildId)
        {
            using DBContext c = new DBContext();

            Permission perm = c.Permission.FirstOrDefault(p => p.DiscordUserId == (long)discordUserId);

            short access = perm?.AccessLevel ?? 0;

            if (discordGuildId != 0)
            {
                DiscordGuild guild = DiscordHandler.Client.GetGuildAsync(discordGuildId).Result;

                if (guild != null)
                {
                    DiscordMember member = guild.GetMemberAsync(discordUserId).Result;

                    if (member != null)
                    {
                        List<DiscordRoleBind> binds = new List<DiscordRoleBind>();
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
                }
            }
            else if (access < (short)AccessLevel.Dev)
                access = 0;

            return (AccessLevel)access;
        }

        /// <summary>
        /// Sets the accesslevel of a user
        /// </summary>
        /// <param name="newAccess">New <see cref="AccessLevel"/></param>
        public static void SetAccessLevel(ulong discordUserId, ulong discordGuildId, AccessLevel newAccess)
        {
            using DBContext c = new DBContext();
            Permission perm = c.Permission.FirstOrDefault(p => p.DiscordUserId == (long)discordUserId);

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
    }
}
