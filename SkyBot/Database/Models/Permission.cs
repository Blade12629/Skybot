using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class Permission : IEquatable<Permission>
    {
        public long Id { get; set; }
        public long DiscordGuildId { get; set; }
        public long DiscordUserId { get; set; }
        public short AccessLevel { get; set; }

        public Permission(long discordUserId, long discordGuildId, AccessLevel accessLevel) : this(discordUserId, discordGuildId, (short)accessLevel)
        {
        }

        public Permission(long discordUserId, long discordGuildId, short accessLevel)
        {
            DiscordUserId = discordUserId;
            DiscordGuildId = discordGuildId;
            AccessLevel = accessLevel;
        }

        public Permission()
        {
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Permission);
        }

        public bool Equals([AllowNull] Permission other)
        {
            return other != null &&
                   Id == other.Id &&
                   DiscordGuildId == other.DiscordGuildId &&
                   DiscordUserId == other.DiscordUserId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, DiscordGuildId, DiscordUserId);
        }

        public static bool operator ==(Permission left, Permission right)
        {
            return EqualityComparer<Permission>.Default.Equals(left, right);
        }

        public static bool operator !=(Permission left, Permission right)
        {
            return !(left == right);
        }
    }
}
