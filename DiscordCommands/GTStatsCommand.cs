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
    public class GTStatsCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "gtstats";

        public AccessLevel AccessLevel => AccessLevel.Dev;

        public bool AllowOverwritingAccessLevel => false;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Display Global Tourney Stats";

        public string Usage => "{prefix}gtstats exampleprofile/ep\n" +
                               "{prefix}gtstats get <osuId>\n";

        public int MinParameters => 1;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            switch(args.Parameters[0].ToLower(CultureInfo.CurrentCulture))
            {
                case "exampleprofile":
                case "ep":
                    args.Channel.SendMessageAsync(embed: GSStatisticHandler.GetTestProfile()).ConfigureAwait(false);
                    break;

                case "get":
                    args.Parameters.RemoveAt(0);
                    GetUser(args);
                    break;

                default:
                    HelpCommand.ShowHelp(args.Channel, this);
                    return;
            }
        }

        private void GetUser(CommandEventArg args)
        {
            if (args.Parameters.Count == 0)
            {
                HelpCommand.ShowHelp(args.Channel, this, Resources.NotEnoughParameters);
                return;
            }
            if (!long.TryParse(args.Parameters[0], out long osuId))
            {
                HelpCommand.ShowHelp(args.Channel, this, "Could not parse osu id");
                return;
            }

            args.Channel.SendMessageAsync(embed: GSStatisticHandler.BuildPlayerProfile(osuId)).ConfigureAwait(false);
        }
    }
}
