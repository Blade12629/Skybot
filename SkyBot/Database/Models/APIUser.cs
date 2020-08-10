using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class APIUser
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
    }
}
