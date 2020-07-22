using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.API.V1.Json
{
    public class JsonGame
    {
        [JsonProperty("game_id")]
        public int GameId { get; set; }

        [JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [JsonProperty("end_time")]
        public DateTime? EndTime { get; set; }

        [JsonProperty("beatmap_id")]
        public int BeatmapId { get; set; }

        // standard = 0, taiko = 1, ctb = 2, o!m = 3
        [JsonProperty("play_mode")]
        public int PlayMode { get; set; }

        [JsonProperty("match_type")]
        public int MatchType { get; set; }

        [JsonProperty("scoring_type")]
        public int ScoringType { get; set; }

        [JsonProperty("team_type")]
        public int TeamType { get; set; }

        [JsonProperty("mods")]
        public int Mods { get; set; }

        [JsonProperty("scores")]
        public JsonScore[] Scores { get; set; }

        public JsonGame(int gameId, DateTime startTime, DateTime? endTime, int beatmapId, 
                        int playMode, int matchType, int scoringType, int teamType, 
                        int mods, JsonScore[] scores)
        {
            GameId = gameId;
            StartTime = startTime;
            EndTime = endTime;
            BeatmapId = beatmapId;
            PlayMode = playMode;
            MatchType = matchType;
            ScoringType = scoringType;
            TeamType = teamType;
            Mods = mods;
            Scores = scores;
        }

        public JsonGame()
        {
        }
    }
}
