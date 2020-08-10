using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class Permission
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
    }
}
