using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.API.V1.Json
{
    public class JsonMatch
    {
        [JsonProperty("match_id")]
        public int MatchId { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time")]
        public DateTime? EndTime { get; set; }

        public JsonMatch(int matchId, string name, DateTime startTime, DateTime? endTime)
        {
            MatchId = matchId;
            Name = name;
            StartTime = startTime;
            EndTime = endTime;
        }

        public JsonMatch()
        {
        }
    }
}
