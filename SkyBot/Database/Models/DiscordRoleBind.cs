using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class DiscordRoleBind : IEquatable<DiscordRoleBind>
    {
        public long Id { get; set; }
        public long GuildId { get; set; }
        public long RoleId { get; set; }
        public short AccessLevel { get; set; }

        public DiscordRoleBind(long guildId, long roleId, short accessLevel)
        {
            GuildId = guildId;
            RoleId = roleId;
            AccessLevel = accessLevel;
        }

        public DiscordRoleBind()
        {
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DiscordRoleBind);
        }

        public bool Equals([AllowNull] DiscordRoleBind other)
        {
            return other != null &&
                   Id == other.Id &&
                   GuildId == other.GuildId &&
                   RoleId == other.RoleId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, GuildId, RoleId);
        }

        public static bool operator ==(DiscordRoleBind left, DiscordRoleBind right)
        {
            return EqualityComparer<DiscordRoleBind>.Default.Equals(left, right);
        }

        public static bool operator !=(DiscordRoleBind left, DiscordRoleBind right)
        {
            return !(left == right);
        }
    }
}
