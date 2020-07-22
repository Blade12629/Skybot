using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.Statistics
{
    public class SeasonTeamCardCache
    {
        public long Id { get; set; }
        public long DiscordGuildId { get; set; }

        public string TeamName { get; set; }
        public string MVPName { get; set; }

        public int TotalMatchMVPs { get; set; }

        public double AverageOverallRating { get; set; }
        public double AverageGeneralPerformanceScore { get; set; }
        public double AverageAccuracy { get; set; }
        public double AverageScore { get; set; }
        public double AverageMisses { get; set; }
        public double AverageCombo { get; set; }

        public double TeamRating { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}
