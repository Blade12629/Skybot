using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.Statistics
{
    public class SeasonPlayer
    {
        public long Id { get; set; }

        public long OsuUserId { get; set; }
        public string LastOsuUsername { get; set; }
        public string TeamName { get; set; }
        public long DiscordGuildId { get; set; }
    }
}
