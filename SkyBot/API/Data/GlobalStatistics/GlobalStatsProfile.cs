using SkyBot.API.Network;
using SkyBot.Database.Models.GlobalStatistics;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.API.Data.GlobalStatistics
{
    public class GlobalStatsProfile : IBinaryAPISerializable
    {
        public string Username { get; set; }
        public long OsuId { get; set; }
        public double BWSRank { get; set; }
        public int TournamentWins { get; set; }
        public string LastPlacement { get; set; }
        public int TournamentsPlayed { get; set; }
        public int BadgeCount { get; set; }
        public DateTime LastUpdated { get; set; }

        public GlobalStatsProfile(string username, long osuId, double bWSRank, int tournamentWins, string lastPlacement, int tournamentsPlayed, int badgeCount, DateTime lastUpdated)
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

        public GlobalStatsProfile()
        {

        }

        public static explicit operator GlobalStatsProfile(PlayerProfile profile)
        {
            return new GlobalStatsProfile(profile.Username, profile.OsuId, profile.BWSRank, profile.TournamentWins, profile.LastPlacement, profile.TournamentsPlayed, profile.BadgeCount, profile.LastUpdated);
        }

        public void Deserialize(BinaryAPIReader reader)
        {
            if (reader == null)
                throw new ArgumentNullException(nameof(reader));

            Username = reader.ReadString();
            OsuId = reader.ReadLong();
            BWSRank = reader.ReadDouble();
            TournamentWins = reader.ReadInt();
            LastPlacement = reader.ReadString();
            TournamentsPlayed = reader.ReadInt();
            BadgeCount = reader.ReadInt();
            LastUpdated = reader.ReadDate();
        }

        public void Serialize(BinaryAPIWriter writer)
        {
            if (writer == null)
                throw new ArgumentNullException(nameof(writer));

            writer.Write(Username);
            writer.Write(OsuId);
            writer.Write(BWSRank);
            writer.Write(TournamentWins);
            writer.Write(LastPlacement);
            writer.Write(TournamentsPlayed);
            writer.Write(BadgeCount);
            writer.Write(LastUpdated);
        }
    }
}
