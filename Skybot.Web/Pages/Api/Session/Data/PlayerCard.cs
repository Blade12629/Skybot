using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkyBot.Database.Models.Statistics;

namespace Skybot.Web.Pages.Api.Session.Data
{
    public struct PlayerCard
    {
        public long OsuUserId { get; set; }
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

        public PlayerCard(long osuUserId, string username, string teamName, double averageAccuracy, double averageScore, double averageMisses, double averageCombo, double averagePerformance, double overallRating, int matchMvps, DateTime lastUpdated) : this()
        {
            OsuUserId = osuUserId;
            Username = username;
            TeamName = teamName;
            AverageAccuracy = averageAccuracy;
            AverageScore = averageScore;
            AverageMisses = averageMisses;
            AverageCombo = averageCombo;
            AveragePerformance = averagePerformance;
            OverallRating = overallRating;
            MatchMvps = matchMvps;
            LastUpdated = lastUpdated;
        }

        public static implicit operator PlayerCard(SeasonPlayerCardCache c)
        {
            return new PlayerCard(c.OsuUserId, c.Username, c.TeamName, c.AverageAccuracy, c.AverageScore, c.AverageMisses, c.AverageCombo, c.AveragePerformance, c.OverallRating, c.MatchMvps, c.LastUpdated);
        }
    }
}
