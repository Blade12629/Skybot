using OsuHistoryEndPoint;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Analyzer.Results
{
    public class BeatmapPlayCount
    {
        public HistoryJson.BeatMap BeatMap { get; set; }
        public int Count { get; set; }
    }
}
