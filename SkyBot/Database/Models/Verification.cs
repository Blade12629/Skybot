using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models
{
    public class Verification
    {
        public long Id { get; set; }
        public long DiscordUserId { get; set; }
        public string VerificationCode { get; set; }

        public Verification(long discordUserId, string verificationCode)
        {
            DiscordUserId = discordUserId;
            VerificationCode = verificationCode;
        }

        public Verification()
        {
        }
    }
}
