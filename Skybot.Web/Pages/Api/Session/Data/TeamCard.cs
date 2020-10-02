using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkyBot.Database.Models.Statistics;

namespace Skybot.Web.Pages.Api.Session.Data
{
    public struct TeamCard
    {
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

        public TeamCard(string teamName, string mVPName, int totalMatchMVPs, double averageOverallRating, double averageGeneralPerformanceScore, double averageAccuracy, double averageScore, double averageMisses, double averageCombo, double teamRating, DateTime lastUpdated) : this()
        {
            TeamName = teamName;
            MVPName = mVPName;
            TotalMatchMVPs = totalMatchMVPs;
            AverageOverallRating = averageOverallRating;
            AverageGeneralPerformanceScore = averageGeneralPerformanceScore;
            AverageAccuracy = averageAccuracy;
            AverageScore = averageScore;
            AverageMisses = averageMisses;
            AverageCombo = averageCombo;
            TeamRating = teamRating;
            LastUpdated = lastUpdated;
        }

        public static implicit operator TeamCard(SeasonTeamCardCache c)
        {
            return new TeamCard(c.TeamName, c.MVPName, c.TotalMatchMVPs, c.AverageOverallRating, c.AverageGeneralPerformanceScore, c.AverageAccuracy, c.AverageScore, c.AverageMisses, c.AverageCombo, c.TeamRating, c.LastUpdated);
        }
    }
}
