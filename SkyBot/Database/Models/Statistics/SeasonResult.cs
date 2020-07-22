using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.Statistics
{
    public class SeasonResult
    {
        public long Id { get; set; }

        public string Stage { get; set; }
        public long MatchId { get; set; }
        public string MatchName { get; set; }
        public long DiscordGuildId { get; set; }

        public string WinningTeam { get; set; }
        public int WinningTeamWins { get; set; }
        public int WinningTeamColor { get; set; }
        public string LosingTeam { get; set; }
        public int LosingTeamWins { get; set; }
        public DateTime TimeStamp { get; set; }
    }
}
