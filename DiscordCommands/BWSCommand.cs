using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class BWSCommand : ICommand
    {
        public string Command => "bws";

        public string Description => "Calculates the bws rank";

        public string Usage => "!bws <rank> <badgeCount>";

        public bool IsDisabled { get; set; }

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => throw new NotImplementedException();

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count < 2)
            {
                HelpCommand.ShowHelp(args.Channel, this);
                return;
            }

            if (!int.TryParse(args.Parameters[0], out int rank))
            {
                HelpCommand.ShowHelp(args.Channel, this, $"Could not parse the rank {args.Parameters[0]}");
                return;
            }

            if (!int.TryParse(args.Parameters[1], out int badge))
            {
                HelpCommand.ShowHelp(args.Channel, this, $"Could not parse the badge count {args.Parameters[1]}");
                return;
            }

            args.Channel.SendMessageAsync($"{args.User.Mention} BWS: {CalculateBWS(rank, badge)}");
        }

        private double CalculateBWS(int rank, int badges)
        {
            return Math.Round(Math.Pow(rank, Math.Pow(0.9921, badges * (badges + 1) / 2)), 4, MidpointRounding.AwayFromZero);
        }
    }
}
