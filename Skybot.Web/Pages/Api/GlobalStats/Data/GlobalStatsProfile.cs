using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skybot.Web.Pages.Api.GlobalStats.Data
{
    public struct GlobalStatsProfile
    {
        public string Username { get; }
        public long OsuId { get; }
        public double BWSRank { get; }
        public int TournamentWins { get; }
        public string LastPlacement { get; }
        public int TournamentsPlayed { get; }
        public int BadgeCount { get; }
        public DateTime LastUpdated { get; }

        public GlobalStatsProfile(string username, long osuId, double bWSRank, int tournamentWins, string lastPlacement, int tournamentsPlayed, int badgeCount, DateTime lastUpdated) : this()
        {
            Username = username;
            OsuId = osuId;
            BWSRank = bWSRank;
            TournamentWins = tournamentWins;
            LastPlacement = lastPlacement;
            TournamentsPlayed = tournamentsPlayed;
            BadgeCount = badgeCount;
            LastUpdated = lastUpdated;
        }

        public static implicit operator GlobalStatsProfile(SkyBot.Database.Models.GlobalStatistics.PlayerProfile pp)
        {
            return new GlobalStatsProfile(pp.Username, pp.OsuId, pp.BWSRank, pp.TournamentWins, pp.LastPlacement, pp.TournamentsPlayed, pp.BadgeCount, pp.LastUpdated);
        }
    }
}
