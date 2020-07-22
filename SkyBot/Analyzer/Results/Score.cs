using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Analyzer.Results
{
    public class Score
    {
        public int beatmapID { get { return UserBeatMap.id.HasValue ? UserBeatMap.id.Value : -1; } }
        public string BeatmapName { get { return $"{UserBeatMap.beatmapset.artist} - {UserBeatMap.beatmapset.title}"; } }
        public string Difficulty { get { return UserBeatMap.version; } }
        public double StarRating { get { return UserBeatMap.difficulty_rating; } }
        public string UserName { get; set; }
        public int User_id { get { return UserScore.user_id ?? 0; } }
        public float Acc { get; set; }
        public OsuHistoryEndPoint.HistoryJson.Score UserScore { get; set; }
        public OsuHistoryEndPoint.HistoryJson.BeatMap UserBeatMap { get; set;}

        public Score(OsuHistoryEndPoint.HistoryJson.Score score, OsuHistoryEndPoint.HistoryJson.BeatMap beatmap)
        {
            UserScore = score;
            UserBeatMap = beatmap;
        }
    }
}
