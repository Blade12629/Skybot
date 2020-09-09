using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.Web
{
    public class WebRateLimit : Ratelimits.QueueRateLimiter
    {
        public static WebRateLimit RateLimit => _rateLimit;
        private static WebRateLimit _rateLimit = new WebRateLimit();

        public WebRateLimit() : base(0, 1, TimeSpan.FromMilliseconds(750))
        {

        }


    }
}
