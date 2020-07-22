using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class BannedUser : IEquatable<BannedUser>
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

        public override bool Equals(object obj)
        {
            return Equals(obj as BannedUser);
        }

        public bool Equals([AllowNull] BannedUser other)
        {
            return other != null &&
                   Id == other.Id &&
                   DiscordUserId == other.DiscordUserId &&
                   DiscordGuildId == other.DiscordGuildId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, DiscordUserId, DiscordGuildId);
        }

        public static bool operator ==(BannedUser left, BannedUser right)
        {
            return EqualityComparer<BannedUser>.Default.Equals(left, right);
        }

        public static bool operator !=(BannedUser left, BannedUser right)
        {
            return !(left == right);
        }
    }
}
