using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class DiscordRoleBind
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
    }
}
