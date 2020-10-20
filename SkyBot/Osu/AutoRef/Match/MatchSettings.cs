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
        public TimeSpan MatchEndDelay { get; set; } = TimeSpan.FromSeconds(240);
        public TimeSpan PlayersReadyUpDelay { get; set; } = TimeSpan.FromSeconds(120);


        public string MatchName { get; set; }

        public string CaptainRed { get; set; }
        public string CaptainBlue { get; set; }
        public string[] PlayersRed { get; set; }
        public string[] PlayersBlue { get; set; }

        public int TotalPlayers => (PlayersBlue?.Length ?? 0) + 1 + (PlayersRed?.Length ?? 0) + 1;
        public int TotalRounds { get; set; }
        public int TotalWarmups { get; set; }

        public bool IsTestRun { get; set; }
        public ulong SubmissionChannel { get; set; }
    }
}
