using SkyBot.Analyzer.Enums;
using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Text;
using OsuHistoryEndPoint.Data;

namespace SkyBot.Analyzer.Results
{
    public class AnalyzerResult
    {
        public int MatchId { get; set; }
        public string Stage { get; set; }
        public string MatchName { get; set; }
        public string WinningTeam { get; set; }
        public int WinningTeamWins { get; set; }
        public string LosingTeam { get; set; }
        public int LosingTeamWins { get; set; }
        public TeamColor WinningTeamColor { get; set; }
        public DateTime TimeStamp { get; set; }
        public (string, string) TeamNames { get; set; }
        
        public HistoryBeatmap[] Beatmaps { get; set; }


        public Rank[] Ranks { get; set; }
        public Rank[] HighestAverageAccuracyRanking { get; set; }
        public Rank[] HighestScoresRanking { get; set; }

        
        public HistoryBeatmap HighestScoreBeatmap { get; set; }

        public HistoryScore HighestScore { get; set; }
        public Player HighestScoreUser { get; set; }

        public HistoryBeatmap HighestAccuracyBeatmap { get; set; }
        public Player HighestAccuracyUser { get; set; }
        public HistoryScore HighestAccuracyScore { get; set; }
        /// <summary>
        /// Beatmapid, score
        /// </summary>
        public (long, HistoryScore)[] Scores { get; set; }

        public Player MVP { get; set; }
    }
}
