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

        public string Command => ResourcesCommands.HelpCommand;

        public AccessLevel AccessLevel => AccessLevel.User;
        public CommandType CommandType => CommandType.None;

        public string Description => ResourcesCommands.HelpCommandDescription;

        public string Usage => ResourcesCommands.HelpCommandUsage;


        public HelpCommand()
        {
            Program.DiscordHandler.CommandHandler.OnException += ShowHelp;
            
#pragma warning disable CA1305 // Specify IFormatProvider
            Logger.Log(string.Format(System.Globalization.CultureInfo.CurrentCulture, ResourcesCommands.RegisteredCommand, nameof(HelpCommand)));
#pragma warning restore CA1305 // Specify IFormatProvider
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

            List<ICommand> commands = handler.Commands.Values.ToList();
            page--;

            double dtotalPages = (double)commands.Count / (double)elementsPerPage;
            int totalPages = (int)(dtotalPages > (int)dtotalPages ? (int)dtotalPages + 1 : dtotalPages);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = ResourcesCommands.CommandList,
                Description = $"{ResourcesCommands.HelpCommandPage}: {page + 1}/{totalPages}",
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

            builder.AddField(ResourcesCommands.Command, cmdBuilder.ToString(), true);
            builder.AddField(Resources.Access, accessBuilder.ToString(), true);
            builder.AddField(ResourcesCommands.CommandDescription, descriptionBuilder.ToString(), true);

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
                Title = $"{ResourcesCommands.CommandInfo}: {command.Command}",
                Timestamp = DateTime.UtcNow,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = ResourcesCommands.HelpCommandFooter
                }
            };

            if (!string.IsNullOrEmpty(notice))
                builder = builder.AddField($"**{ResourcesCommands.HelpCommandNotice}**", notice);

            builder = builder.AddField(Resources.AccessLevel, command.AccessLevel.ToString())
                             .AddField(ResourcesCommands.CommandDescription, command.Description)
                             .AddField(ResourcesCommands.CommandUsage, command.Usage)
                             .AddField(ResourcesCommands.CommandType, command.CommandType.ToString())
                             .AddField(ResourcesCommands.CommandIsDisabled, command.IsDisabled ? Resources.True : Resources.False);


            channel.SendMessageAsync(embed: builder.Build()).Wait();
        }
    }
}
