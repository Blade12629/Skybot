using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.Web
{
    public class WebUser
    {
        public long Id { get; set; }
        public long DiscordUserId { get; set; }
        public bool AllowGlobalStats { get; set; }

        public WebUser(long discordUserId, bool allowGlobalStats)
        {
            DiscordUserId = discordUserId;
            AllowGlobalStats = allowGlobalStats;
        }

        public WebUser()
        {

        }
    }
}
