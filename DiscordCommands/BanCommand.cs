using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
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

        public string Description => "Bans a user locally/globally";

        public string Usage => "{prefix}ban <discordUserId> <osuUserId> <reason>\n" +
                               "{prefix}ban global <discordUserId> <osuUserId> <reason> (Only available for dev role)\n" +
                               "{prefix}ban list local/global [page, default: 1]\n" +
                               "For now if you want to unban someone contact us on discord";

        public int MinParameters => 2;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            bool globalBan = false;
            //discordId, osuId
            (ulong, ulong) ids = ParseIds(args, ref globalBan);

            if (ids.Item1 == 0 && ids.Item2 == 0)
            {
                if (args.Parameters[0].Equals("list", StringComparison.CurrentCultureIgnoreCase) && TryListBans(args))
                    return;

                HelpCommand.ShowHelp(args.Channel, this, "Failed to parse parameters");
                return;
            }
            else if (globalBan && args.AccessLevel < AccessLevel.Dev)
            {
                HelpCommand.ShowHelp(args.Channel, this, "You do not have enough permissions to use this command");
                return;
            }

            string reason = globalBan ? args.ParameterString.Remove(0, args.Parameters[0].Length + args.Parameters[1].Length + args.Parameters[2].Length + 2).TrimStart(' ') :
                                        args.ParameterString.Remove(0, args.Parameters[0].Length + args.Parameters[1].Length + 1).TrimStart(' ');

            if (string.IsNullOrEmpty(reason))
                reason = null;

            BanManager.BanUser((long)ids.Item1, (long)ids.Item2, globalBan ? 0 : (long)args.Guild.Id, reason);
        }

        private bool TryListBans(CommandEventArg args)
        {
            List<BannedUser> bans = BanManager.GetBans(guildId: args.Parameters.Count < 2 ? 0 :
                                                                    args.Parameters[1].Equals("global", StringComparison.CurrentCultureIgnoreCase) ? 0 :
                                                                        args.Parameters[1].Equals("local", StringComparison.CurrentCultureIgnoreCase) ? (long)args.Guild.Id : 0);

            if (bans.Count == 0)
            {
                HelpCommand.ShowHelp(args.Channel, this, "No bans found");
                return true;
            }

            int page = 1;
            if (args.Parameters.Count >= 3 && int.TryParse(args.Parameters[2], out int page_))
                page = page_;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "Ban list",
                Description = Resources.InvisibleCharacter
            };
            EmbedPageBuilder epb = new EmbedPageBuilder(3);
            epb.AddColumn("Discord ID");
            epb.AddColumn("Osu ID");
            epb.AddColumn("Reason");

            for (int i = 0; i < bans.Count; i++)
            {
                epb.Add("Discord ID", bans[i].DiscordUserId.ToString(CultureInfo.CurrentCulture));
                epb.Add("Osu ID", bans[i].OsuUserId.ToString(CultureInfo.CurrentCulture));
                epb.Add("Reason", string.IsNullOrEmpty(bans[i].Reason) ? "none" : bans[i].Reason);
            }

            DiscordEmbed embed = epb.BuildPage(builder, page);
            args.Channel.SendMessageAsync(embed: embed).ConfigureAwait(false);

            return true;
        }

        private (ulong, ulong) ParseIds(CommandEventArg args, ref bool globalBan)
        {
            if (!ulong.TryParse(args.Parameters[0], out ulong discordUserId))
            {
                if (!args.Parameters[0].Equals("global", StringComparison.CurrentCultureIgnoreCase))
                    return (0, 0);

                globalBan = true;

                if (!ulong.TryParse(args.Parameters[1], out discordUserId))
                    return (0, 0);

                if (!ulong.TryParse(args.Parameters[2], out ulong osuUserId))
                    return (0, 0);

                return (discordUserId, osuUserId);
            }

            if (!ulong.TryParse(args.Parameters[1], out ulong osuUserId_))
                return (0, 0);

            return (discordUserId, osuUserId_);
        }
    }
}
