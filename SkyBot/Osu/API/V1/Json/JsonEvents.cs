using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.API.V1.Json
{
    public class JsonEvents
    {
        [JsonProperty("display_html")]
        public string DisplayHtml { get; set; }

        [JsonProperty("beatmap_id")]
        public int? BeatmapId { get; set; }

        [JsonProperty("beatmapset_id")]
        public int? BeatmapsetId { get; set; }

        [JsonProperty("date")]
        public DateTime? Date { get; set; }

        [JsonProperty("epicfactor")]
        public int? EpicFactor { get; set; }

        public JsonEvents(string displayHtml, int beatmapId, int beatmapsetId, DateTime date, int epicFactor)
        {
            DisplayHtml = displayHtml;
            BeatmapId = beatmapId;
            BeatmapsetId = beatmapsetId;
            Date = date;
            EpicFactor = epicFactor;
        }

        public JsonEvents()
        {
        }
    }
}
