using OsuHistoryEndPoint.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Analyzer.Results
{
    public class Score
    {
        public int beatmapID { get { return UserBeatMap.Id; } }
        public string BeatmapName { get { return $"{UserBeatMap.Beatmapset.Artist} - {UserBeatMap.Beatmapset.Title}"; } }
        public string Difficulty { get { return UserBeatMap.Version; } }
        public double StarRating { get { return UserBeatMap.DifficultyRating; } }
        public string UserName { get; set; }
        public int UserId { get { return UserScore.UserId; } }
        public float Acc { get; set; }
        public HistoryScore UserScore { get; set; }
        public HistoryBeatmap UserBeatMap { get; set;}

        public Score(HistoryScore score, HistoryBeatmap beatmap)
        {
            UserScore = score;
            UserBeatMap = beatmap;
        }
    }
}
