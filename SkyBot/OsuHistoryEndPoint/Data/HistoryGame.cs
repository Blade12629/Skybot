using System;
using Newtonsoft.Json;

namespace OsuHistoryEndPoint.Data
{
    public class HistoryGame
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time")]
        public DateTime? EndTime { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("mode_int")]
        public int ModeInt { get; set; }

        [JsonProperty("scoring_type")]
        public string ScoringType { get; set; }

        [JsonProperty("team_type")]
        public string TeamType { get; set; }

        [JsonProperty("mods")]
        public string[] Mods { get; set; }

        [JsonProperty("beatmap")]
        public HistoryBeatmap Beatmap { get; set; }

        [JsonProperty("scores")]
        public HistoryScore[] Scores { get; set; }
    }
}
