using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class ConfigCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "config";

        public AccessLevel AccessLevel => AccessLevel.Admin;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Sets/Gets/Lists config settings";

        public string Usage => "!config set <key> <value>\n!config get <key>\n!config list";

        public int MinParameters => 1;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count > 1)
            {
                switch(args.Parameters[1].ToLower(System.Globalization.CultureInfo.CurrentCulture))
                {
                    case "guildid":
                    case "id":
                        args.Channel.SendMessageAsync("This is a reserved key, you cannot change this");
                        return;
                }
            }

            using DBContext c = new DBContext();
            switch(args.Parameters[0].ToLower(System.Globalization.CultureInfo.CurrentCulture))
            {
                case "set":
                    args.ParameterString = args.ParameterString.Remove(0, args.Parameters[0].Length).TrimStart(' ');
                    args.Parameters.RemoveAt(0);
                    Set(args, c);
                    break;

                case "get":
                    args.ParameterString = args.ParameterString.Remove(0, args.Parameters[0].Length).TrimStart(' ');
                    args.Parameters.RemoveAt(0);
                    Get(args);
                    break;

                case "list":
                    args.ParameterString = args.ParameterString.Remove(0, args.Parameters[0].Length).TrimStart(' ');
                    args.Parameters.RemoveAt(0);
                    List(args);
                    break;

                default:
                    HelpCommand.ShowHelp(args.Channel, this);
                    break;

            }
        }

        private void List(CommandEventArg args)
        {
            StringBuilder response = new StringBuilder();

            foreach (var key in DiscordGuildConfig.GetKeys())
                response.AppendLine(key);

            args.Channel.SendMessageAsync(response.ToString());
        }

        private void Set(CommandEventArg args, DBContext c)
        {
            DiscordGuildConfig dgc = args.Config;

            if (dgc == null)
            {
                dgc = c.DiscordGuildConfig.Add(new DiscordGuildConfig()
                {
                    GuildId = (long)args.Guild.Id
                }).Entity;
                c.SaveChanges();
            }

            string val = (args.Parameters.Count == 2 ? args.Parameters[1] : args.ParameterString.Remove(0, args.Parameters[0].Length)).TrimStart(' ').TrimEnd(' ');

            bool result = dgc.TrySetValue(args.Parameters[0], val);

            c.DiscordGuildConfig.Update(dgc);
            c.SaveChanges();

            if (result)
                args.Channel.SendMessageAsync("Set value");
            else
                args.Channel.SendMessageAsync("Failed to set value");
        }

        private void Get(CommandEventArg args)
        {
            DiscordGuildConfig dgc = args.Config;

            if (dgc == null)
                dgc = new DiscordGuildConfig();

            args.Channel.SendMessageAsync(dgc.TryGetValue(args.Parameters[0]));
        }
    }
}
