using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class BannedGuild : IEquatable<BannedGuild>
    {
        public long Id { get; set; }
        public long DiscordGuildId { get; set; }
        public string Reason { get; set; }

        public BannedGuild(long discordGuildId, string reason)
        {
            DiscordGuildId = discordGuildId;
            Reason = reason;
        }

        public BannedGuild()
        {
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as BannedGuild);
        }

        public bool Equals([AllowNull] BannedGuild other)
        {
            return other != null &&
                   Id == other.Id &&
                   DiscordGuildId == other.DiscordGuildId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, DiscordGuildId);
        }

        public static bool operator ==(BannedGuild left, BannedGuild right)
        {
            return EqualityComparer<BannedGuild>.Default.Equals(left, right);
        }

        public static bool operator !=(BannedGuild left, BannedGuild right)
        {
            return !(left == right);
        }
    }
}
