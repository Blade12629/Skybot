using SkyBot.Analyzer.Enums;
using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Text;

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
        
        public HistoryJson.BeatMap[] Beatmaps { get; set; }


        public Rank[] Ranks { get; set; }
        public Rank[] HighestAverageAccuracyRanking { get; set; }
        public Rank[] HighestScoresRanking { get; set; }

        
        public HistoryJson.BeatMap HighestScoreBeatmap { get; set; }

        public HistoryJson.Score HighestScore { get; set; }
        public Player HighestScoreUser { get; set; }

        public HistoryJson.BeatMap HighestAccuracyBeatmap { get; set; }
        public Player HighestAccuracyUser { get; set; }
        public HistoryJson.Score HighestAccuracyScore { get; set; }
        /// <summary>
        /// Beatmapid, score
        /// </summary>
        public (long, HistoryJson.Score)[] Scores { get; set; }

        public Player MVP { get; set; }
    }
}
