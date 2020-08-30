using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models
{
    public class CommandAccess
    {
        public long Id { get; set; }
        public long DiscordGuildId { get; set; }
        public string TypeName { get; set; }
        public int AccessLevel { get; set; }

        public CommandAccess(long discordGuildId, string typeName, int accessLevel)
        {
            DiscordGuildId = discordGuildId;
            TypeName = typeName;
            AccessLevel = accessLevel;
        }

        public CommandAccess()
        {
        }
    }
}
