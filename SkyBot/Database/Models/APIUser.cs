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
        public string APIKeyMD5 { get; set; }
        public bool IsValid { get; set; }

        public APIUser(long discordUserId, string apiKeyMD5)
        {
            DiscordUserId = discordUserId;
            APIKeyMD5 = apiKeyMD5;
            IsValid = true;
        }

        public APIUser()
        {
        }
    }
}
