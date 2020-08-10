using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class BannedUser
    {
        public long Id { get; set; }
        public long DiscordUserId { get; set; }
        /// <summary>
        /// Null if blacklisted for every server
        /// </summary>
        public long? DiscordGuildId { get; set; }
        public string Reason { get; set; }

        public BannedUser(long discordUserId, long? discordGuildId, string reason)
        {
            DiscordUserId = discordUserId;
            DiscordGuildId = discordGuildId;
            Reason = reason;
        }

        public BannedUser()
        {
        }
    }
}
