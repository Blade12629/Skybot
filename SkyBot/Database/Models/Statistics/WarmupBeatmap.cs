using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.Statistics
{
    public class WarmupBeatmap
    {
        public long Id { get; set; }
        public long DiscordGuildId { get; set; }
        public long BeatmapId { get; set; }
    }
}
