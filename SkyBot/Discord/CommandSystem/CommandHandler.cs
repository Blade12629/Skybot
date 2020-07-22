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
    public class CommandHandler : IDisposable
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

            if (cmd == null || !_commandTypes.TryAdd(cmd.Command.ToLower(), cmd))
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
            command = command.TrimStart(CommandPrefix).ToLower();

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
                if (!e.Message.Content[0].Equals(CommandPrefix))
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

                if (!_commandTypes.TryGetValue(command.ToLower(), out ICommand cmd))
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
                    catch (Exception ex)
                    {
                        Logger.Log($"Something went wrong while invoking command {cmd.Command}: {ex.ToString()}");
                        OnException?.Invoke(e.Channel, cmd, "Something went wrong executing this command");
                    }
                }));

            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogLevel.Error); 
            }
        }

        /// <summary>
        /// Gets the access level of the user, either directly or via roles
        /// </summary>
        public AccessLevel GetAccessLevel(ulong discordUserId, ulong discordGuildId = 0)
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

            return (AccessLevel)access;
        }

        /// <summary>
        /// Gets the access level of the user, either directly or via roles
        /// </summary>
        public AccessLevel GetAccessLevel(DiscordUser user, DiscordGuild guild = null)
        {
            return GetAccessLevel(user.Id, guild?.Id ?? 0);
        }

        /// <summary>
        /// Gets the access level of the user, either directly or via roles
        /// </summary>
        public AccessLevel GetAccessLevel(DiscordMember member)
        {
            return GetAccessLevel(member.Id, member.Guild.Id);
        }
    }
}
