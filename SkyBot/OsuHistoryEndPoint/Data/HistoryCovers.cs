﻿using Newtonsoft.Json;

namespace OsuHistoryEndPoint.Data
{
    public class HistoryCovers
    {
        [JsonProperty("cover")]
        public string Cover { get; set; }

        [JsonProperty(@"cover@2x")]
        public string Cover2x { get; set; }

        [JsonProperty("card")]
        public string Card { get; set; }

        [JsonProperty("card@2x")]
        public string Card2x { get; set; }

        [JsonProperty("list")]
        public string List { get; set; }

        [JsonProperty("list@2x")]
        public string List2x { get; set; }

        [JsonProperty("slimcover")]
        public string Slimcover { get; set; }

        [JsonProperty("slimcover@2x")]
        public string Slimcover2x { get; set; }
    }
}
