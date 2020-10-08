using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Match
{
    public class MatchSettings
    {
        public DateTime MatchStartTime { get; set; }
        public TimeSpan MatchCreationDelay { get; set; }
        public TimeSpan MatchInviteDelay { get; set; }

        public string MatchName { get; set; }

        public string CaptainRed { get; set; }
        public string CaptainBlue { get; set; }
        public string[] PlayersRed { get; set; }
        public string[] PlayersBlue { get; set; }
    }
}
