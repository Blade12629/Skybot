using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.API.V1.Json
{/// <summary>
 /// Parameters: k* api key, u* userid/username, m mode (0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania) (default: 0), type* UseName/UseID (string, id) (preferred: UseID) , event_days (1-31)
 /// </summary>
    public class JsonGetUser
    {
        [JsonProperty("user_id")]
        public int UserId { get; set; }

        [JsonProperty("username")]
        public string UserName { get; set; }

        [JsonProperty("count300")]
        public int Count300 { get; set; }

        [JsonProperty("count100")]
        public int Count100 { get; set; }

        [JsonProperty("count50")]
        public int Count50 { get; set; }

        [JsonProperty("playcount")]
        public int PlayCount { get; set; }

        [JsonProperty("ranked_score")]
        public string RankedScore { get; set; }

        [JsonProperty("total_score")]
        public string TotalScore { get; set; }

        [JsonProperty("pp_rank")]
        public int PPRank { get; set; }

        [JsonProperty("level")]
        public float Level { get; set; }

        [JsonProperty("pp_raw")]
        public float PPRaw { get; set; }

        [JsonProperty("accuracy")]
        public float Accuracy { get; set; }

        [JsonProperty("count_rank_ss")]
        public int CountRankSS { get; set; }

        [JsonProperty("count_rank_ssh")]
        public int CountRankSSH { get; set; }

        [JsonProperty("count_rank_s")]
        public int CountRankS { get; set; }

        [JsonProperty("count_rank_sh")]
        public int CountRankSH { get; set; }

        [JsonProperty("count_rank_a")]
        public int CountRankA { get; set; }

        [JsonProperty("country")]
        public string Country { get; set; }

        [JsonProperty("pp_country_rank")]
        public int PPCountryRank { get; set; }
        
        [JsonProperty("events")]
        public JsonEvents[] Events { get; set; }

        public JsonGetUser(int userId, string userName, int count300, int count100, int count50, 
                           int playCount, string rankedScore, string totalScore, int pPRank, 
                           float level, float pPRaw, float accuracy, int countRankSS, int countRankSSH, 
                           int countRankS, int countRankSH, int countRankA, string country, 
                           int pPCountryRank, JsonEvents[] events)
        {
            UserId = userId;
            UserName = userName;
            Count300 = count300;
            Count100 = count100;
            Count50 = count50;
            PlayCount = playCount;
            RankedScore = rankedScore;
            TotalScore = totalScore;
            PPRank = pPRank;
            Level = level;
            PPRaw = pPRaw;
            Accuracy = accuracy;
            CountRankSS = countRankSS;
            CountRankSSH = countRankSSH;
            CountRankS = countRankS;
            CountRankSH = countRankSH;
            CountRankA = countRankA;
            Country = country;
            PPCountryRank = pPCountryRank;
            Events = events;
        }

        public JsonGetUser()
        {
        }
    }
}
