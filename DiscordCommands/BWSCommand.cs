using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DiscordCommands
{
    public class BWSCommand : ICommand
    {
        public string Command => ResourcesCommands.BWSCommand;

        public string Description => ResourcesCommands.BWSCommandDescription;

        public string Usage => ResourcesCommands.BWSCommandUsage;

        public bool IsDisabled { get; set; }

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count < 2)
            {
                HelpCommand.ShowHelp(args.Channel, this);
                return;
            }

            if (!int.TryParse(args.Parameters[0], out int rank))
            {
                HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, ResourcesCommands.BWSCommandFailedParseRank, args.Parameters[0]));
                return;
            }

            if (!int.TryParse(args.Parameters[1], out int badgeCount))
            {
                HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, ResourcesCommands.BWSCommandFailedParseBadgeCount, args.Parameters[1]));
                return;
            }

            args.Channel.SendMessageAsync(string.Format(CultureInfo.CurrentCulture, ResourcesCommands.BWSCommandResult, args.User.Mention, CalculateBWS(rank, badgeCount)));
        }

        private double CalculateBWS(int rank, int badgeCount)
        {
            return Math.Round(Math.Pow(rank, Math.Pow(0.9921, badgeCount * (badgeCount + 1.0) / 2.0)), 4, MidpointRounding.AwayFromZero);
        }
    }
}
