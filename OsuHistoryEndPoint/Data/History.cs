using Newtonsoft.Json;
using System;

namespace OsuHistoryEndPoint.Data
{
    public class History
    {
        [JsonProperty("events")]
        public HistoryEvent[] Events { get; set; }

        [JsonProperty("users")]
        public HistoryUser[] Users { get; set; }

        [JsonProperty("latest_event_id")]
        public int LatestEventId { get; set; }

        [JsonProperty("current_game_id")]
        public long? CurrentGameId { get; set; }
    }
}
