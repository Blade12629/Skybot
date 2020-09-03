using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class BanCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "ban";

        public AccessLevel AccessLevel => AccessLevel.Host;

        public bool AllowOverwritingAccessLevel => false;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Bans a user locally (Only available for dev role)";

        public string Usage => "{prefix}ban <discordUserId> <osuUserId> <reason>\n" +
                               "{prefix}ban global <discordUserId> <osuUserId> <reason> (Only available for dev role)\n" +
                               "For now if you want to unban someone contact us on discord";

        public int MinParameters => 3;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            bool globalBan = false;
            //discordId, osuId
            (ulong, ulong) ids = ParseIds(args, ref globalBan);

            if (ids.Item1 == 0 && ids.Item2 == 0)
                return;
            else if (globalBan && args.AccessLevel < AccessLevel.Dev)
            {
                HelpCommand.ShowHelp(args.Channel, this, Resources.AccessTooLow);
                return;
            }

            string reason = globalBan ? args.ParameterString.Remove(0, args.Parameters[0].Length + args.Parameters[1].Length + args.Parameters[2].Length + 2).TrimStart(' ') :
                                        args.ParameterString.Remove(0, args.Parameters[0].Length + args.Parameters[1].Length + 1).TrimStart(' ');

            if (string.IsNullOrEmpty(reason))
                reason = null;

            BanManager.BanUser((long)ids.Item1, (long)ids.Item2, globalBan ? 0 : (long)args.Guild.Id, reason);
        }

        private (ulong, ulong) ParseIds(CommandEventArg args, ref bool globalBan)
        {
            if (!ulong.TryParse(args.Parameters[0], out ulong discordUserId))
            {
                if (!args.Parameters[0].Equals("global", StringComparison.CurrentCultureIgnoreCase))
                {
                    HelpCommand.ShowHelp(args.Channel, this, "Failed to parse discord user id and could not find global");
                    return (0, 0);
                }

                globalBan = true;

                if (!ulong.TryParse(args.Parameters[1], out discordUserId))
                {
                    HelpCommand.ShowHelp(args.Channel, this, "Failed to parse discord user id");
                    return (0, 0);
                }

                if (!ulong.TryParse(args.Parameters[2], out ulong osuUserId))
                {
                    HelpCommand.ShowHelp(args.Channel, this, "Failed to parse osu user id");
                    return (0, 0);
                }

                return (discordUserId, osuUserId);
            }

            if (!ulong.TryParse(args.Parameters[1], out ulong osuUserId_))
            {
                HelpCommand.ShowHelp(args.Channel, this, "Failed to parse osu user id");
                return (0, 0);
            }

            return (discordUserId, osuUserId_);
        }
    }
}
