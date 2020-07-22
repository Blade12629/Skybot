using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class APIUser : IEquatable<APIUser>
    {
        public long Id { get; set; }
        public long DiscordUserId { get; set; }
        public long DiscordGuildId { get; set; }
        public string APIKeyMD5 { get; set; }

        public APIUser(long discordUserId, long discordGuildId, string aPIKeyMD5)
        {
            DiscordUserId = discordUserId;
            DiscordGuildId = discordGuildId;
            APIKeyMD5 = aPIKeyMD5;
        }

        public APIUser()
        {
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as APIUser);
        }

        public bool Equals([AllowNull] APIUser other)
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

        public static bool operator ==(APIUser left, APIUser right)
        {
            return EqualityComparer<APIUser>.Default.Equals(left, right);
        }

        public static bool operator !=(APIUser left, APIUser right)
        {
            return !(left == right);
        }
    }
}
