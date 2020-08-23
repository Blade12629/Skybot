using Newtonsoft.Json;

namespace OsuHistoryEndPoint.Data
{
    public class HistoryBeatmap
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("mode")]
        public string Mode { get; set; }

        [JsonProperty("difficulty_rating")]
        public double DifficultyRating { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("beatmapset")]
        public HistoryBeatmapSet Beatmapset { get; set; }
    }
}
