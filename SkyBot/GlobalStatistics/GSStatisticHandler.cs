using DSharpPlus.Entities;
using SkyBot.Database.Models;
using SkyBot.Database.Models.GlobalStatistics;
using SkyBot.Osu.Web;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace SkyBot.GlobalStatistics
{
    public static class GSStatisticHandler
    {
        /// <summary>
        /// Builds a player profile
        /// </summary>
        public static DiscordEmbed BuildPlayerProfile(long osuId)
        {
            using DBContext c = new DBContext();
            PlayerProfile p = c.PlayerProfile.FirstOrDefault(p => p.OsuId == osuId);

            if (p == null)
            {
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                {
                    Title = "No stats found"
                };

                return builder.Build();
            }

            return BuildPlayerProfile(p.Username, osuId, p.LastUpdated, p.BWSRank, p.TournamentWins, p.LastPlacement, p.TournamentsPlayed, p.BadgeCount);
        }

        /// <summary>
        /// Gets a test profile embed
        /// </summary>
        public static DiscordEmbed GetTestProfile()
        {
            double bwsRank = CalculateBWS(5000, 3);
            return BuildPlayerProfile("Skyfly", 5790241, DateTime.UtcNow, bwsRank, 4, "30th", 6, 3);
        }

        /// <summary>
        /// Submits a gs tournament
        /// </summary>
        /// <returns>row id</returns>
        public static long SubmitGSTournament(long hostOsuId, string tournamentName, string acronym, string thread, string countryCode, DateTime start, DateTime end, int rankMin, int rankMax)
        {
            if (hostOsuId <= 0)
                throw new ArgumentOutOfRangeException(nameof(hostOsuId));
            else if (string.IsNullOrEmpty(tournamentName))
                throw new ArgumentNullException(nameof(tournamentName));
            else if (string.IsNullOrEmpty(acronym))
                throw new ArgumentNullException(nameof(acronym));
            else if (string.IsNullOrEmpty(thread))
                throw new ArgumentNullException(nameof(thread));
            else if (string.IsNullOrEmpty(countryCode))
                throw new ArgumentNullException(nameof(countryCode));
            else if (start == DateTime.MinValue)
                throw new ArgumentOutOfRangeException(nameof(start));
            else if (end == DateTime.MinValue)
                throw new ArgumentOutOfRangeException(nameof(end));

            using DBContext c = new DBContext();
            var ent = c.GSTournament.Add(new Database.Models.GlobalStatistics.GSTournament(hostOsuId, tournamentName, acronym, thread, countryCode, start, end, rankMin, rankMax)).Entity;
            c.SaveChanges();

            return ent.Id;
        }

        /// <summary>
        /// Submits a gs team
        /// </summary>
        /// <returns>row id</returns>
        public static long SubmitGSTeam(long gsTournamentId, int placement, string teamName)
        {
            if (placement <= 0)
                throw new ArgumentOutOfRangeException(nameof(placement));
            else if (string.IsNullOrEmpty(teamName))
                throw new ArgumentNullException(nameof(teamName));

            using DBContext c = new DBContext();
            var ent = c.GSTeam.Add(new Database.Models.GlobalStatistics.GSTeam(gsTournamentId, placement, teamName)).Entity;
            c.SaveChanges();

            return ent.Id;
        }

        /// <summary>
        /// Submits a gs team member
        /// </summary>
        public static void SubmitGSTeamMember(long gsTeamId, long osuUserId)
        {
            if (osuUserId <= 0)
                throw new ArgumentOutOfRangeException(nameof(osuUserId));

            using DBContext c = new DBContext();
            c.GSTeamMember.Add(new Database.Models.GlobalStatistics.GSTeamMember(gsTeamId, osuUserId));
            c.SaveChanges();
        }

        /// <summary>
        /// Updates the player profile with a specific gs tournament id
        /// </summary>
        public static void UpdatePlayerProfiles(long gsTournamentId)
        {
            using DBContext c = new DBContext();
            GSTournament tournament = c.GSTournament.FirstOrDefault(t => t.Id == gsTournamentId);

            if (tournament == null)
                return;

            List<GSTeam> teams = c.GSTeam.Where(t => t.GSTournamentId == tournament.Id).ToList();
            List<GSTeamMember> members = new List<GSTeamMember>();

            for (int i = 0; i < teams.Count; i++)
            {
                foreach(GSTeamMember member in c.GSTeamMember.Where(t => t.GSTeamId == teams[i].Id))
                {
                    try
                    {
                        PlayerProfile p = c.PlayerProfile.FirstOrDefault(p => p.OsuId == member.OsuUserId);

                        if (p == null)
                        {
                            p = c.PlayerProfile.Add(new PlayerProfile()
                            {
                                OsuId = member.OsuUserId,
                                Username = "",
                                LastPlacement = "",
                                LastUpdated = DateTime.UtcNow
                            }).Entity;

                            c.SaveChanges();
                        }

                        p.LastPlacement = teams[i].Placement.ToString(CultureInfo.CurrentCulture);
                        p.TournamentsPlayed++;

                        if (teams[i].Placement == 1)
                            p.TournamentWins++;

                        (int, string) rankAndUsername = GetRankAndUsername(member.OsuUserId);

                        p.BadgeCount = GetBadgeCount(member.OsuUserId);
                        p.BWSRank = CalculateBWS(rankAndUsername.Item1, p.BadgeCount);
                        p.Username = rankAndUsername.Item2;
                        p.LastUpdated = DateTime.UtcNow;

                        c.PlayerProfile.Update(p);
                        c.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Logger.Log($"Error building player profile, skipping: {ex}");
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// Builds a player profile
        /// </summary>
        /// <returns>player profile embed</returns>
        public static DiscordEmbed BuildPlayerProfile(string username, long osuId, DateTime lastUpdated, double bwsRank,
                                                      int tournamentWins, string lastPlacement, int tournamentsPlayed, int badgeCount)
        {
            DiscordColor color = new DiscordColor((float)Program.Random.NextDouble(), (float)Program.Random.NextDouble(), (float)Program.Random.NextDouble());
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder
            {
                Title = $"Tournament Stats for {username}",
                Url = $"https://osu.ppy.sh/users/{osuId}",
                Color = color,
                Timestamp = lastUpdated,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = "Last Updated"
                },
                ThumbnailUrl = $"https://a.ppy.sh/{osuId}",
            };

            builder.AddField(Resources.InvisibleCharacter, $"**- BWS Rank:** {Math.Round(bwsRank, 2, MidpointRounding.AwayFromZero)}\n**- Tournament Wins:** {tournamentWins}\n**- Last Placement:** {lastPlacement}\n**- Tournaments Played:** {tournamentsPlayed}\n**- Badge Count:** {badgeCount}");

            return builder.Build();
        }

        /// <summary>
        /// Adds or updates stats
        /// </summary>
        /// <param name="osuId"></param>
        /// <param name="tournamentWins">-1 = don't change</param>
        /// <param name="lastPlacement"></param>
        /// <param name="tournamentsPlayed">-1 = don't change</param>
        /// <param name="badgeCount"></param>
        public static void AddOrUpdateStats(long osuId, int tournamentWins = -1, string lastPlacement = null,
                                       int tournamentsPlayed = -1)
        {
            using DBContext c = new DBContext();
            PlayerProfile prof = c.PlayerProfile.FirstOrDefault(p => p.OsuId == osuId);
            (int, string) rankAndUsername = GetRankAndUsername(osuId);

            int badges = GetBadgeCount(osuId);

            if (prof == null)
            {
                double bwsRank = CalculateBWS(rankAndUsername.Item1, badges);

                c.PlayerProfile.Add(new PlayerProfile()
                {
                    OsuId = osuId,
                    Username = rankAndUsername.Item2,
                    TournamentWins = tournamentWins == -1 ? 0 : tournamentWins,
                    LastPlacement = lastPlacement == null ? Resources.InvisibleCharacter : lastPlacement,
                    TournamentsPlayed = tournamentsPlayed == -1 ? 0 : tournamentsPlayed,
                    BadgeCount = badges,
                    BWSRank = bwsRank,
                    LastUpdated = DateTime.UtcNow
                });

                c.SaveChanges();
                return;
            }

            if (tournamentWins > -1)
                prof.TournamentWins = tournamentWins;
            if (lastPlacement != null)
                prof.LastPlacement = lastPlacement;
            if (tournamentsPlayed > -1)
                prof.TournamentsPlayed = tournamentsPlayed;


            prof.BadgeCount = badges;
            prof.Username = rankAndUsername.Item2;
            prof.BWSRank = CalculateBWS(rankAndUsername.Item1, badges);
            prof.LastUpdated = DateTime.UtcNow;
            c.PlayerProfile.Update(prof);
            c.SaveChanges();
        }

        /// <summary>
        /// Gets the rank and username from a specific osu user id
        /// </summary>
        private static (int, string) GetRankAndUsername(long osuId)
        {
            var user = Osu.API.V1.OsuApi.GetUser((int)osuId).ConfigureAwait(false).GetAwaiter().GetResult();
            return (user.PPRank, user.UserName);
        }

        /// <summary>
        /// Calculates the BWS Rank
        /// </summary>
        public static double CalculateBWS(int rank, int badges)
        {
            const double BADGE_PUNISH_COUNT = 5.0;
            const double BASE_ADDITION_VALUE = 0.0;
            const double CAPPED_BADGE_VALUE = 1.3;
            const double BWS_MOD = 0.99;
            const int DEFAULT_UPPER_RANK_LIMIT = 50000;
            const int DEFAULT_UPPER_BADGE_LIMIT = 100000;

            double badgeValue = badges * (badges / BADGE_PUNISH_COUNT);
            double badgePercent = Math.Min(BASE_ADDITION_VALUE + badgeValue * badges, 200);

            double ratio = 0;

            for (int i = 0; i < 8; i++)
            {
                double bwsRatio = GetBWSRatio(DEFAULT_UPPER_RANK_LIMIT, badgePercent, CAPPED_BADGE_VALUE, DEFAULT_UPPER_BADGE_LIMIT);
                ratio += bwsRatio * (i == 0 ? 1.0 : (i * 0.1));
            }

            return Math.Pow(rank, Math.Pow(BWS_MOD, Math.Pow(ratio, 2)));
        }

        /// <summary>
        /// Gets the badge count for a specific user
        /// </summary>
        public static int GetBadgeCount(long osuId)
        {
            using (BadgeGrabber bg = new BadgeGrabber())
            {
                object badgeCount = -1;

                WebRateLimit.RateLimit.Increment(() =>
                {
                    badgeCount = bg.Count(osuId);
                }, o =>
                {
                    badgeCount = bg.Count(osuId);
                }, badgeCount);

                while ((int)badgeCount == -1)
                    System.Threading.Tasks.Task.Delay(5).ConfigureAwait(false).GetAwaiter().GetResult();

                return (int)badgeCount;
            }
        }

        /// <summary>
        /// Gets the bws ratio
        /// </summary>
        private static double GetBWSRatio(int upperRankLimit, double badgePercent, double cappedBadgeValue, int upperBadgeLimit)
        {
            double bwsRatio = upperRankLimit / (double)upperBadgeLimit + (upperRankLimit / (double)upperBadgeLimit) * (badgePercent / 100);

            return Math.Max(Math.Min(bwsRatio, cappedBadgeValue), 0);
        }
    }
}
