using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.API.V1.Json
{
    public class JsonScore
    {
        [JsonProperty("slot")]
        public int Slot { get; set; }
        [JsonProperty("team")]
        public int Team { get; set; }
        [JsonProperty("user_id")]
        public int UserId { get; set; }
        [JsonProperty("score")]
        public int Score { get; set; }
        [JsonProperty("maxcombo")]
        public int MaxCombo { get; set; }
        [JsonProperty("rank")]
        public int Rank { get; set; }
        [JsonProperty("count50")]
        public int Count50 { get; set; }
        [JsonProperty("count100")]
        public int Count100 { get; set; }
        [JsonProperty("count300")]
        public int Count300 { get; set; }
        [JsonProperty("countmiss")]
        public int CountMiss { get; set; }
        [JsonProperty("countgeki")]
        public int CountGeki { get; set; }
        [JsonProperty("countkatu")]
        public int CountKatu { get; set; }
        [JsonProperty("perfect")]
        public int Perfect { get; set; }
        [JsonProperty("pass")]
        public int Pass { get; set; }

        public JsonScore(int slot, int team, int userId, int score, int maxCombo, 
                         int rank, int count50, int count100, int count300, 
                         int countMiss, int countGeki, int countKatu, int perfect, 
                         int pass)
        {
            Slot = slot;
            Team = team;
            UserId = userId;
            Score = score;
            MaxCombo = maxCombo;
            Rank = rank;
            Count50 = count50;
            Count100 = count100;
            Count300 = count300;
            CountMiss = countMiss;
            CountGeki = countGeki;
            CountKatu = countKatu;
            Perfect = perfect;
            Pass = pass;
        }

        public JsonScore()
        {
        }
    }
}
