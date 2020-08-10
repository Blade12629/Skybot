using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Discord.Data
{
    public class Footer
    {
#pragma warning disable CA1056 // Uri properties should not be strings
        public string IconUrl { get; set; }
#pragma warning restore CA1056 // Uri properties should not be strings
        public string Text { get; set; }
    }
}
