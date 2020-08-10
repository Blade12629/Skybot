using Newtonsoft.Json;

namespace OsuHistoryEndPoint.Data
{
    public class HistoryDetail
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("text")]
        public string MatchName { get; set; }
    }
}
