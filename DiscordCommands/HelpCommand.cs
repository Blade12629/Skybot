using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

[assembly: CLSCompliant(false)]

namespace DiscordCommands
{
    public class HelpCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "help";

        public AccessLevel AccessLevel => AccessLevel.User;
        public CommandType CommandType => CommandType.None;

        public string Description => "Displays a command list or infos about a specific command";

        public string Usage => "{prefix}help [page]\n" +
                               "{prefix}help <command>";

        public int MinParameters => 0;
        public bool AllowOverwritingAccessLevel => false;


        public HelpCommand()
        {
            Program.DiscordHandler.CommandHandler.OnException += ShowHelp;
            
#pragma warning disable CA1305 // Specify IFormatProvider
            Logger.Log($"Registered {nameof(HelpCommand)} for discord command exceptions");
#pragma warning restore CA1305 // Specify IFormatProvider
        }

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            char prefix = args.Config?.Prefix ?? Program.DiscordHandler.CommandHandler.CommandPrefix;

            int page = 1;
            if (args.Parameters.Count > 0)
            {
                if (char.IsDigit(args.Parameters[0][0]))
                {
                    if (int.TryParse(args.Parameters[0], out int page_))
                    {
                        args.Parameters.RemoveAt(0);
                        page = page_;
                    }
                }

                if (args.Parameters.Count > 0 && ShowHelp(handler, args, prefix))
                    return;
            }

            ListCommands(handler, args, prefix, page);
        }

        private void ListCommands(CommandHandler handler, CommandEventArg args, char prefix, int page = 1)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            else if (args == null)
                throw new ArgumentNullException(nameof(args));

            List<(ICommand, AccessLevel)> commands = handler.Commands.Values.Select(s => (s, s.AccessLevel)).ToList();

            for (int i = 0; i < commands.Count; i++)
            {
                AccessLevel newAccess = CommandHandler.GetCommandAccessLevel(commands[i].Item1, args.Guild?.Id ?? 0);

                if (newAccess > args.AccessLevel)
                {
                    commands.RemoveAt(i);
                    i--;
                    continue;
                }

                commands[i] = (commands[i].Item1, newAccess);
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "Command List",
                Description = "Prefix: " + prefix,
                Timestamp = DateTime.UtcNow
            };

            EmbedPageBuilder epb = new EmbedPageBuilder(3);
            epb.AddColumn("Command");
            epb.AddColumn("Access");
            epb.AddColumn("Description");

            for (int i = 0; i < commands.Count; i++)
            {
                epb.Add("Command", commands[i].Item1.Command);
                epb.Add("Access", commands[i].Item2.ToString());
                epb.Add("Description", commands[i].Item1.Description);
            }

            DiscordEmbed embed = epb.BuildPage(builder, page);

            args.Channel.SendMessageAsync(embed: embed).Wait();

            return;
        }


        private static bool ShowHelp(CommandHandler handler, CommandEventArg args, char prefix, string notice = null)
        {
            if (handler == null)
                throw new ArgumentNullException(nameof(handler));
            else if (args == null)
                throw new ArgumentNullException(nameof(args));

            string command = args.Parameters[0].Trim('!');

            if (handler.Commands.TryGetValue(command, out ICommand cmd))
            {
                ShowHelp(args.Channel, cmd, prefix, notice);
                return true;
            }

            return false;
        }

        public static void ShowHelp(DiscordChannel channel, ICommand command, string notice = null)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            else if (command == null)
                throw new ArgumentNullException(nameof(command));

            using DBContext c = new DBContext();
            DiscordGuildConfig dgc = null;

            if (channel.Guild != null)
                dgc = c.DiscordGuildConfig.FirstOrDefault(dgc => dgc.GuildId == (long)channel.Guild.Id);

            char prefix = dgc?.Prefix ?? Program.DiscordHandler.CommandHandler.CommandPrefix;

            ShowHelp(channel, command, prefix, notice);
        }

        public static void ShowHelp(DiscordChannel channel, ICommand command, char prefix, string notice = null)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            else if (command == null)
                throw new ArgumentNullException(nameof(command));

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = $"Command Info: {command.Command}",
                Timestamp = DateTime.UtcNow,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text =  "< > = required\n" +
                            "[ ] = optional\n" +
                            "/ = choose between\n" +
                            "!! !! = atleast one marked parameter required"
                }
            };

            if (!string.IsNullOrEmpty(notice))
            {
                if (notice.Length > 1000)
                    notice = notice.Substring(0, 1000);

                builder = builder.AddField($"**Notice**", notice);
            }

            builder = builder.AddField("Access Level", CommandHandler.GetCommandAccessLevel(command, channel.Guild?.Id ?? 0).ToString())
                             .AddField("Description", command.Description)
                             .AddField("Usage", command.Usage.Replace("{prefix}", prefix.ToString(CultureInfo.CurrentCulture), StringComparison.CurrentCultureIgnoreCase))
                             .AddField("Type", command.CommandType.ToString())
                             .AddField("Is Disabled", command.IsDisabled ? "True" : "False")
                             .AddField("Access can be overwritten", command.AllowOverwritingAccessLevel ? "True" : "False");


            channel.SendMessageAsync(embed: builder.Build()).Wait();
        }
    }
}
