using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.Web
{
    public class BadgeInfo
    {
        [JsonProperty("awarded_at")]
        public DateTime AwardedAt { get; set; }
        [JsonProperty("description")]
        public string Description { get; set; }
        [JsonProperty("image_url")]
#pragma warning disable CA1056 // Uri properties should not be strings
        public string ImageUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings

#pragma warning disable CA1054 // Uri parameters should not be strings
        public BadgeInfo(DateTime awardedAt, string description, string imageUrl)
#pragma warning restore CA1054 // Uri parameters should not be strings
        {
            AwardedAt = awardedAt;
            Description = description;
            ImageUrl = imageUrl;
        }

        public BadgeInfo()
        {
        }
    }
}
