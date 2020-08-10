using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class BannedGuild
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
    }
}
