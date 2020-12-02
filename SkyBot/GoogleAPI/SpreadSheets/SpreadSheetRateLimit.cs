using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.GoogleAPI.SpreadSheets
{
    /// <summary>
    /// Singleton, use <see cref="SpreadSheetRateLimit.RateLimit"/>
    /// </summary>
    public class SpreadSheetRateLimit : Ratelimits.QueueRateLimiter
    {
        public static SpreadSheetRateLimit RateLimit { get; } = new SpreadSheetRateLimit();

        private SpreadSheetRateLimit() : base(0, 100, TimeSpan.FromSeconds(101))
        {
        }
    }
}
