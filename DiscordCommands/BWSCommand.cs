using SkyBot;
using SkyBot.Discord;
using SkyBot.Discord.CommandSystem;
using SkyBot.GlobalStatistics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DiscordCommands
{
    public class BWSCommand : ICommand
    {
        public string Command => "bws";

        public string Description => "Calculates the bws rank";

        public string Usage => "{prefix}bws <rank> <badgeCount>";

        public bool IsDisabled { get; set; }

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        public int MinParameters => 2;
        public bool AllowOverwritingAccessLevel => true;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            if (!int.TryParse(args.Parameters[0], out int rank))
            {
                HelpCommand.ShowHelp(args.Channel, this, $"Could not parse the rank {args.Parameters[0]}");
                return;
            }

            if (!int.TryParse(args.Parameters[1], out int badgeCount))
            {
                HelpCommand.ShowHelp(args.Channel, this, $"Could not parse the badge count {args.Parameters[1]}");
                return;
            }

            if (args.Parameters.Count >= 3 && args.Parameters[2].Equals("test", StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            client.SendSimpleEmbed(args.Channel, "BWS Rank", GSStatisticHandler.CalculateBWS(rank, badgeCount).ToString()).ConfigureAwait(false);
        }
    }
}
