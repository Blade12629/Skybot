using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.Statistics
{
    public class SeasonScore
    {
        public long Id { get; set; }

        public long BeatmapId { get; set; }
        public long SeasonPlayerId { get; set; }
        public long SeasonResultId { get; set; }
        public string TeamName { get; set; }

        public bool TeamVs { get; set; }
        /// <summary>
        /// 1. map = 1, 2. map = 2, etc.
        /// </summary>
        public int PlayOrder { get; set; }
        public double GeneralPerformanceScore { get; set; }
        public bool HighestGeneralPerformanceScore { get; set; }

        public float Accuracy { get; set; }

        public long Score { get; set; }
        public int MaxCombo { get; set; }
        public int Perfect { get; set; }
        public DateTime PlayedAt { get; set; }
        public int Pass { get; set; }
        public int Count50 { get; set; }
        public int Count100 { get; set; }
        public int Count300 { get; set; }
        public int CountGeki { get; set; }
        public int CountKatu { get; set; }
        public int CountMiss { get; set; }
    }
}
