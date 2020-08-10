using System;
using Newtonsoft.Json;

namespace OsuHistoryEndPoint.Data
{
    public class HistoryEvent
    {
        [JsonProperty("id")]
        public int? EventId { get; set; }

        [JsonProperty("detail")]
        public HistoryDetail Detail { get; set; }

        [JsonProperty("timestamp")]
        public DateTime TimeStamp { get; set; }

        [JsonProperty("user_id")]
        public int? UserId { get; set; }

        [JsonProperty("game")]
        public HistoryGame Game { get; set; }
    }
}
