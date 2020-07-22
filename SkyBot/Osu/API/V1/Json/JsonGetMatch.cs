using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.API.V1.Json
{
    public class JsonGetMatch
    {
        [JsonProperty("match")]
        public JsonMatch Match { get; set; }

        [JsonProperty("games")]
        public JsonGame[] Games { get; set; }

        public JsonGetMatch(JsonMatch match, JsonGame[] games)
        {
            Match = match;
            Games = games;
        }

        public JsonGetMatch()
        {
        }
    }
}
