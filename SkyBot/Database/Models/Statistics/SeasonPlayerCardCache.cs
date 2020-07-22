using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.Statistics
{
    public class SeasonPlayerCardCache
    {
        public long Id { get; set; }
        public long OsuUserId { get; set; }
        public long DiscordGuildId { get; set; }
        public string Username { get; set; }

        public string TeamName { get; set; }

        public double AverageAccuracy { get; set; }
        public double AverageScore { get; set; }
        public double AverageMisses { get; set; }
        public double AverageCombo { get; set; }

        public double AveragePerformance { get; set; }
        public double OverallRating { get; set; }

        public int MatchMvps { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
