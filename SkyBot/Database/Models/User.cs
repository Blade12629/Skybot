using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Database.Models
{
    public class User
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
    }
}
