using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SkyBot.Database;
using SkyBot.Database.Models;

namespace SkyBot
{
    public static class BanManager
    {
        public static List<BannedUser> GetBansForUser(long discordId = 0, long osuId = 0, long guildId = 0)
        {
            if (discordId <= 0 && osuId <= 0)
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentOutOfRangeException("discordId+osuId");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

            using DBContext c = new DBContext();
            List<BannedUser> result = new List<BannedUser>();

            if (discordId != 0)
                result.AddRange(c.BannedUser.Where(u => u.DiscordUserId == discordId));
            if (osuId != 0)
                result.AddRange(c.BannedUser.Where(u => u.OsuUserId == osuId));
            if (guildId != 0)
                result.AddRange(c.BannedUser.Where(u => u.DiscordGuildId == guildId));

            return result;
        }

        public static void UnbanUser(long discordId = 0, long osuId = 0, long guildId = 0)
        {
            if (discordId <= 0 && osuId <= 0)
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
                throw new ArgumentOutOfRangeException("discordId+osuId");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

            List<BannedUser> bannedUsers = GetBansForUser(discordId, osuId, guildId);

            if (bannedUsers.Count == 0)
                return;

            using DBContext c = new DBContext();
            c.BannedUser.RemoveRange(bannedUsers);
        }

        public static void BanUser(long discordId = 0, long osuId = 0, long guildId = 0, string reason = null)
        {
            using DBContext c = new DBContext();
            List<BannedUser> bans = GetBansForUser(discordId, osuId, guildId);

            if (bans.Count == 0)
            {
                c.BannedUser.Add(new BannedUser(osuId, discordId, guildId, reason));
                c.SaveChanges();
                return;
            }

            BannedUser user = bans.FirstOrDefault(u => (discordId > 0 ? discordId == u.DiscordUserId : true) &&
                                                       (osuId > 0 ? osuId == u.OsuUserId : true) &&
                                                       (guildId > 0 ? guildId == u.DiscordGuildId : true));

            if (user != null)
                return;

            c.BannedUser.Add(new BannedUser(discordId, osuId, guildId, reason));
            c.SaveChanges();
        }
    }
}
