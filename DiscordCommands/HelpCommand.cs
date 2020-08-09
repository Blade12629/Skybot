using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Discord;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[assembly: CLSCompliant(false)]

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
            List<ICommand> commands = handler.Commands.Values.ToList();

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = ResourcesCommands.CommandList,
                Timestamp = DateTime.UtcNow
            };

            EmbedPageBuilder epb = new EmbedPageBuilder(3);
            epb.AddColumn(ResourcesCommands.Command);
            epb.AddColumn(Resources.Access);
            epb.AddColumn(ResourcesCommands.CommandDescription);

            for (int i = 0; i < commands.Count; i++)
            {
                epb.Add(ResourcesCommands.Command, commands[i].Command);
                epb.Add(Resources.Access, commands[i].AccessLevel.ToString());
                epb.Add(ResourcesCommands.CommandDescription, commands[i].Description);
            }

            DiscordEmbed embed = epb.BuildPage(builder, page);

            args.Channel.SendMessageAsync(embed: embed).Wait();

            return;
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
