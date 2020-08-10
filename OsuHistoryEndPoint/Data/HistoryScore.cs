using System;
using Newtonsoft.Json;

namespace OsuHistoryEndPoint.Data
{
    public class HistoryScore
    {
        [Obsolete("Always null (01.08.2020)")]
        [JsonProperty("id")]
        public object Id { get; set; }

        [Obsolete("Always null (01.08.2020)")]
        [JsonProperty("best_id")]
        public object BestId { get; set; }

        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("accuracy")]
        public double Accuracy { get; set; }

        [JsonProperty("mods")]
        public string[] Mods { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        [JsonProperty("perfect")]
        public int Perfect { get; set; }

        [JsonProperty("statistics")]
        public HistoryStatistics Statistics { get; set; }

        [Obsolete("Always null (01.08.2020)")]
        [JsonProperty("pp")]
        public object PP { get; set; }

        [Obsolete("Always null (01.08.2020)")]
        [JsonProperty("rank")]
        public object Rank { get; set; }

        [Obsolete("Always null (01.08.2020)")]
        [JsonProperty("created_at")]
        public object CreatedAt { get; set; }

        [JsonProperty("match")]
        public HistoryMatch Match { get; set; }
    }
}
