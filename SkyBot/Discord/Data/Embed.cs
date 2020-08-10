using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Discord.Data
{
    public class Embed
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Url { get; set; }
        public int Color { get; set; }
        public DateTime Timestamp { get; set; }
        public Footer Footer { get; set; }
        public Thumbnail Thumbnail { get; set; }
        public Image Image { get; set; }
        public Author Author { get; set; }
#pragma warning disable CA1819 // Properties should not return arrays
        public Field[] Fields { get; set; }
#pragma warning restore CA1819 // Properties should not return arrays
    }
}
