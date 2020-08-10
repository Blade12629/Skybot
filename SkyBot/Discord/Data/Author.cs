using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Discord.Data
{
    public class Author
    {
        public string Name { get; set; }
#pragma warning disable CA1056 // Uri properties should not be strings
        public string Url { get; set; }
        public string IconUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings
    }
}
