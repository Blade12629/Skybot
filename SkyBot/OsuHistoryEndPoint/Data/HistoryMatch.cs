using Newtonsoft.Json;

namespace OsuHistoryEndPoint.Data
{
    public class HistoryMatch
    {
        [JsonProperty("slot")]
        public int Slot { get; set; }

        [JsonProperty("team")]
        public string Team { get; set; }

        [JsonProperty("pass")]
        public int Pass { get; set; }
    }
}
