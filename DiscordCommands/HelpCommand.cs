using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class HelpCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "help";

        public AccessLevel AccessLevel => AccessLevel.User;
        public CommandType CommandType => CommandType.None;

        public string Description => "Displays a command list or infos about a specific command";

        public string Usage => "!help [page]\n!help <command>";


        public HelpCommand()
        {
            Program.DiscordHandler.CommandHandler.OnException += ShowHelp;
            Logger.Log("Registered help for exceptions");
        }

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
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

                if (args.Parameters.Count > 0 && ShowHelp(handler, args))
                    return;
            }

            ListCommands(handler, args, page);
        }

        private void ListCommands(CommandHandler handler, CommandEventArg args, int page = 1)
        {
            const int elementsPerPage = 10;

            //AccessLevel access = CommandHandler.GetAccessLevel(args.User.Id);

            List<ICommand> commands = handler.Commands.Values.ToList();
            page--;

            double dtotalPages = (double)commands.Count / (double)elementsPerPage;
            int totalPages = (int)(dtotalPages > (int)dtotalPages ? (int)dtotalPages + 1 : dtotalPages);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "Command List",
                Description = $"Page: {page + 1}/{totalPages}",
                Timestamp = DateTime.UtcNow
            };

            StringBuilder cmdBuilder = new StringBuilder();
            StringBuilder descriptionBuilder = new StringBuilder();
            StringBuilder accessBuilder = new StringBuilder();

            int end = elementsPerPage * (page + 1);

            for (int i = elementsPerPage * page; i < end && i < commands.Count; i++)
            {
                if (args.AccessLevel < commands[i].AccessLevel)
                {
                    end++;
                    continue;
                }

                cmdBuilder.AppendLine(commands[i].Command);
                descriptionBuilder.AppendLine(commands[i].Description);
                accessBuilder.AppendLine(commands[i].AccessLevel.ToString());

                if (commands[i].Description.Length > 114)
                {
                    for (int x = 0; x < 2; x++)
                    {
                        cmdBuilder.AppendLine();
                        accessBuilder.AppendLine();
                        descriptionBuilder.AppendLine();
                    }
                }
                else if (commands[i].Description.Length > 57)
                {
                    cmdBuilder.AppendLine();
                    accessBuilder.AppendLine();
                    descriptionBuilder.AppendLine();
                }
            }

            builder.AddField("Command", cmdBuilder.ToString(), true);
            builder.AddField("Access", accessBuilder.ToString(), true);
            builder.AddField("Description", descriptionBuilder.ToString(), true);

            args.Channel.SendMessageAsync(embed: builder.Build()).Wait();
        }


        public static bool ShowHelp(CommandHandler handler, CommandEventArg args, string notice = null)
        {
            string command = args.Parameters[0].Trim('!');

            if (handler.Commands.TryGetValue(command, out ICommand cmd))
            {
                ShowHelp(args.Channel, cmd, notice);
                return true;
            }

            return false;
        }

        public static void ShowHelp(DiscordChannel channel, ICommand command, string notice = null)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = $"CommandInfo: {command.Command}",
                Timestamp = DateTime.UtcNow,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "< > = required\n[ ] = optional\n!! !!  = atleast one marked parameter required"
                }
            };

            builder = builder.AddField("Access Level", command.AccessLevel.ToString())
                             .AddField("Description", command.Description)
                             .AddField("Usage", command.Usage)
                             .AddField("Command Type", command.CommandType.ToString())
                             .AddField("IsDisabled", command.IsDisabled ? "True" : "False");

            if (!string.IsNullOrEmpty(notice))
                builder = builder.AddField("**Notice**", notice);

            channel.SendMessageAsync(embed: builder.Build()).Wait();
        }
    }
}
