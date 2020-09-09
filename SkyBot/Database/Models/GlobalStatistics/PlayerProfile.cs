using System;

namespace SkyBot.Database.Models.GlobalStatistics
{
    public class PlayerProfile
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public long OsuId { get; set; }
        public double BWSRank { get; set; }
        public int TournamentWins { get; set; }
        //public string MostPlayedMod { get; set; }
        public string LastPlacement { get; set; }
        public int TournamentsPlayed { get; set; }
        //public int MapsPlayed { get; set; }
        public int BadgeCount { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
