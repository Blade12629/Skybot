using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Osu.API.V1.Json
{
    public class JsonGetBeatmap
    {
        [JsonProperty("approved")]
        public ApprovedEnum Approved { get; set; }
        [JsonProperty("approved_date")]
        public DateTime ApprovedDate { get; set; }
        [JsonProperty("last_update")]
        public DateTime LastUpdate { get; set; }
        [JsonProperty("artist")]
        public string Artist { get; set; }
        [JsonProperty("beatmap_id")]
        public int BeatmapId { get; set; }
        [JsonProperty("beatmapset_id")]
        public int BeatmapsetId { get; set; }
        [JsonProperty("bpm")]
        public int BPM { get; set; }

        [JsonProperty("creator")]
        public string Creator { get; set; }

        [JsonProperty("difficultyrating")]
        public float DifficultRating { get; set; }

        [JsonProperty("diff_size")]
        public float DiffSize { get; set; }

        [JsonProperty("diff_overall")]
        public float DiffOverall { get; set; }

        [JsonProperty("diff_approach")]
        public float DiffApproach { get; set; }

        [JsonProperty("diff_drain")]
        public float DiffDrain { get; set; }

        [JsonProperty("hit_length")]
        public int HitLength { get; set; }

        [JsonProperty("source")]
        public string Source { get; set; }

        [JsonProperty("genre_id")]
        public GenreEnum GenreId { get; set; }

        [JsonProperty("language_id")]
        public LanguageIDEnum LanguageId { get; set; }

        [JsonProperty("title")]
        public string Title { get; set; }

        [JsonProperty("total_length")]
        public int TotalLength { get; set; }

        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("file_md5")]
        public string FileMD5 { get; set; }

        [JsonProperty("mode")]
        public GameModeEnum GameMode { get; set; }

        [JsonProperty("tags")]
        public string Tags { get; set; }

        [JsonProperty("favourite_count")]
        public int FavouriteCount { get; set; }

        [JsonProperty("playcount")]
        public int PlayCount { get; set; }

        [JsonProperty("passcount")]
        public int PassCount { get; set; }

        [JsonProperty("max_combo")]
        public int MaxCombo { get; set; }

        public JsonGetBeatmap(ApprovedEnum approved, DateTime approvedDate, DateTime lastUpdate, string artist, 
                              int beatmapId, int beatmapsetId, int bPM, string creator, float difficultRating, 
                              float diffSize, float diffOverall, float diffApproach, float diffDrain, int hitLength, 
                              string source, GenreEnum genreId, LanguageIDEnum languageId, string title, int totalLength, 
                              string version, string fileMD5, GameModeEnum gameMode, string tags, int favouriteCount, 
                              int playCount, int passCount, int maxCombo)
        {
            Approved = approved;
            ApprovedDate = approvedDate;
            LastUpdate = lastUpdate;
            Artist = artist;
            BeatmapId = beatmapId;
            BeatmapsetId = beatmapsetId;
            BPM = bPM;
            Creator = creator;
            DifficultRating = difficultRating;
            DiffSize = diffSize;
            DiffOverall = diffOverall;
            DiffApproach = diffApproach;
            DiffDrain = diffDrain;
            HitLength = hitLength;
            Source = source;
            GenreId = genreId;
            LanguageId = languageId;
            Title = title;
            TotalLength = totalLength;
            Version = version;
            FileMD5 = fileMD5;
            GameMode = gameMode;
            Tags = tags;
            FavouriteCount = favouriteCount;
            PlayCount = playCount;
            PassCount = passCount;
            MaxCombo = maxCombo;
        }

        public JsonGetBeatmap()
        {
        }
    }
}
