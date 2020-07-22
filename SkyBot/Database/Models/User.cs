using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class User : IEquatable<User>
    {
        public long Id { get; set; }
        public long DiscordUserId { get; set; }
        public long OsuUserId { get; set; }

        public User(long discordUserId, long osuUserId)
        {
            DiscordUserId = discordUserId;
            OsuUserId = osuUserId;
        }

        public User()
        {
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as User);
        }

        public bool Equals([AllowNull] User other)
        {
            return other != null &&
                   Id == other.Id &&
                   DiscordUserId == other.DiscordUserId &&
                   OsuUserId == other.OsuUserId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, DiscordUserId, OsuUserId);
        }

        public static bool operator ==(User left, User right)
        {
            return EqualityComparer<User>.Default.Equals(left, right);
        }

        public static bool operator !=(User left, User right)
        {
            return !(left == right);
        }
    }
}
