using SkyBot.Database.Models.Statistics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.Analyzer
{

    public static class Cache
    {
        /// <summary>
        /// Clears the cache of a player
        /// </summary>
        /// <param name="osuUserId">Osu User Id</param>
        /// <param name="c">DB Context</param>
        /// <param name="guildId">Discord Guild Id</param>
        /// <returns>Empty Cache</returns>
        public static SeasonPlayerCardCache ClearPlayerCache(long osuUserId, DBContext c, long guildId)
        {
            SeasonPlayerCardCache cardCache = c.SeasonPlayerCardCache.FirstOrDefault(cc => cc.OsuUserId == osuUserId &&
                                                                                           cc.DiscordGuildId == guildId);

            if (cardCache == null)
            {
                cardCache = c.SeasonPlayerCardCache.Add(new SeasonPlayerCardCache()
                {
                    OsuUserId = osuUserId,
                    AverageAccuracy = 0,
                    AverageCombo = 0,
                    AverageMisses = 0,
                    AveragePerformance = 0,
                    AverageScore = 0,
                    LastUpdated = DateTime.UtcNow,
                    MatchMvps = 0,
                    OverallRating = 0,
                    TeamName = "null",
                    Username = "null",
                    DiscordGuildId = 0,
                }).Entity;

                c.SaveChanges();
                return cardCache;
            }

            cardCache.AverageAccuracy = 0;
            cardCache.AverageCombo = 0;
            cardCache.AverageMisses = 0;
            cardCache.AveragePerformance = 0;
            cardCache.AverageScore = 0;
            cardCache.LastUpdated = DateTime.UtcNow;
            cardCache.MatchMvps = 0;
            cardCache.OverallRating = 0;
            cardCache.TeamName = "null";
            cardCache.Username = "null";
            cardCache.DiscordGuildId = 0;

            cardCache = c.SeasonPlayerCardCache.Update(cardCache).Entity;
            c.SaveChanges();

            return cardCache;
        }

        /// <summary>
        /// Refreshes or adds a cache for a player
        /// </summary>
        /// <param name="osuUserId">Osu User Id</param>
        /// <param name="c">DB Context</param>
        /// <param name="guildId">Discord Guild Id</param>
        /// <param name="getOverallRatingAction">Function to calculate the overall rating</param>
        public static void ForceRefreshPlayerCache(long osuUserId, DBContext c, long guildId, Func<SeasonPlayer, DBContext, double> getOverallRatingAction)
        {
            SeasonPlayerCardCache cardCache = ClearPlayerCache(osuUserId, c, guildId);

            SeasonPlayer player = c.SeasonPlayer.FirstOrDefault(p => p.OsuUserId == osuUserId && p.DiscordGuildId == guildId);
            List<SeasonResult> results = c.SeasonResult.Where(sr => sr.DiscordGuildId == guildId).ToList();
            List<SeasonScore> scores = c.SeasonScore.Where(s => s.SeasonPlayerId == player.Id && results.Any(sr => s.SeasonResultId == sr.Id)).ToList();

            cardCache.DiscordGuildId = guildId;

            foreach (SeasonScore score in scores)
            {
                cardCache.AverageAccuracy += score.Accuracy * 100.0;
                cardCache.AverageCombo += score.MaxCombo;
                cardCache.AverageMisses += score.CountMiss;
                cardCache.AverageScore += score.Score;
                cardCache.AveragePerformance += score.GeneralPerformanceScore;

                if (score.HighestGeneralPerformanceScore)
                    cardCache.MatchMvps++;
            }

            cardCache.AverageAccuracy /= scores.Count;
            cardCache.AverageCombo /= scores.Count;
            cardCache.AverageMisses /= scores.Count;
            cardCache.AverageScore /= scores.Count;
            cardCache.AveragePerformance /= scores.Count;

            cardCache.OverallRating = getOverallRatingAction(player, c);

            cardCache.LastUpdated = DateTime.UtcNow;
            cardCache.TeamName = player.TeamName;

            cardCache.Username = player.LastOsuUsername ?? "not found";

            c.SeasonPlayerCardCache.Update(cardCache);
            c.SaveChanges();
        }

        /// <summary>
        /// Clears the cache of a team
        /// </summary>
        /// <param name="teamName">Team Name</param>
        /// <param name="c">DB Context</param>
        /// <param name="guildId">Discord Guild Id</param>
        /// <returns>Empty Cache</returns>
        public static SeasonTeamCardCache ClearTeamCache(string teamName, DBContext c, long guildId)
        {
            SeasonTeamCardCache stcc = c.SeasonTeamCardCache.FirstOrDefault(stcc => stcc.TeamName.Equals(teamName, StringComparison.CurrentCultureIgnoreCase) &&
                                                                                    stcc.DiscordGuildId == guildId);

            if (stcc == null)
            {
                stcc = new SeasonTeamCardCache()
                {
                    AverageAccuracy = 0,
                    AverageCombo = 0,
                    AverageGeneralPerformanceScore = 0,
                    AverageScore = 0,
                    AverageMisses = 0,
                    AverageOverallRating = 0,
                    DiscordGuildId = guildId,
                    LastUpdated = DateTime.MinValue,
                    MVPName = "",
                    TeamName = "",
                    TotalMatchMVPs = 0
                };

                stcc = c.SeasonTeamCardCache.Add(stcc).Entity;
                c.SaveChanges();

                return stcc;
            }

            stcc.AverageAccuracy = 0;
            stcc.AverageCombo = 0;
            stcc.AverageGeneralPerformanceScore = 0;
            stcc.AverageScore = 0;
            stcc.AverageMisses = 0;
            stcc.AverageOverallRating = 0;
            stcc.DiscordGuildId = guildId;
            stcc.LastUpdated = DateTime.MinValue;
            stcc.MVPName = "";
            stcc.TeamName = "";
            stcc.TotalMatchMVPs = 0;

            stcc = c.SeasonTeamCardCache.Update(stcc).Entity;
            c.SaveChanges();

            return stcc;
        }

        /// <summary>
        /// Refreshes or adds a cache for a player
        /// </summary>
        /// <param name="teamName">Team Name</param>
        /// <param name="c">DB Context</param>
        /// <param name="guildId">Discord Guild Id</param>
        public static void ForceRefreshTeamCache(string teamName, DBContext c, long guildId)
        {
            SeasonTeamCardCache stcc = ClearTeamCache(teamName, c, guildId);

            List<SeasonResult> results = c.SeasonResult.Where(r => r.DiscordGuildId == guildId &&
                                                                   r.LosingTeam.Equals(teamName, StringComparison.CurrentCultureIgnoreCase) ||
                                                                   r.WinningTeam.Equals(teamName, StringComparison.CurrentCultureIgnoreCase)).ToList();

            Dictionary<long, int> userMvps = new Dictionary<long, int>();

            double avgAcc = 0, avgScore = 0, avgMisses = 0, avgCombo = 0, avgRating = 0, avgGPS = 0, matchMVPs = 0;
            int counter = 0;
            for (int r = 0; r < results.Count; r++)
            {
                List<SeasonScore> scores = c.SeasonScore.Where(s => s.SeasonResultId == results[r].Id &&
                                                                    s.TeamName.Equals(teamName, StringComparison.CurrentCultureIgnoreCase)).ToList();

                if (scores.Count == 0)
                    continue;

                double acc = 0, score = 0, misses = 0, combo = 0, gps = 0, mvps = 0;
                for (int s = 0; s < scores.Count; s++)
                {
                    if (!userMvps.Keys.Contains(scores[s].SeasonPlayerId))
                        userMvps.Add(scores[s].SeasonPlayerId, scores[s].HighestGeneralPerformanceScore ? 1 : 0);
                    else if (scores[s].HighestGeneralPerformanceScore)
                        userMvps[scores[s].SeasonPlayerId]++;

                    avgAcc += scores[s].Accuracy;
                    avgScore += scores[s].Score;
                    avgMisses += scores[s].CountMiss;
                    avgCombo += scores[s].MaxCombo;
                    avgGPS += scores[s].GeneralPerformanceScore;

                    if (scores[s].HighestGeneralPerformanceScore)
                        mvps++;
                }

                avgAcc += acc / scores.Count;
                avgScore = score / scores.Count;
                avgMisses += misses / scores.Count;
                avgCombo += combo / scores.Count;
                avgGPS += gps / scores.Count;
                matchMVPs += mvps;

                counter++;
            }

            avgAcc /= counter;
            avgScore /= counter;
            avgCombo /= counter;
            avgGPS /= counter;

            List<SeasonPlayer> players = c.SeasonPlayer.Where(p => p.DiscordGuildId == guildId &&
                                                                   userMvps.Keys.Contains(p.Id)).ToList();

            List<SeasonPlayerCardCache> cardCaches = c.SeasonPlayerCardCache.Where(spcc => spcc.DiscordGuildId == guildId &&
                                                                                           players.Any(p => p.OsuUserId == spcc.OsuUserId)).ToList();

            for (int i = 0; i < cardCaches.Count; i++)
                avgRating += cardCaches[i].OverallRating;

            avgRating /= cardCaches.Count;

            stcc.TeamName = teamName;

            stcc.TotalMatchMVPs = (int)matchMVPs;
            stcc.AverageOverallRating = avgRating;
            stcc.AverageGeneralPerformanceScore = avgGPS;
            stcc.AverageAccuracy = avgAcc;
            stcc.AverageScore = avgScore;
            stcc.AverageMisses = avgMisses;
            stcc.AverageCombo = avgCombo;
            stcc.MVPName = cardCaches.First(cc => cc.OsuUserId == 
                                                               players.First(p => p.Id == 
                                                                                       userMvps.First(m => m.Value == 
                                                                                                                   userMvps.Max(mp => mp.Value))
                                                                                               .Key)
                                                                      .OsuUserId)
                                     .Username;

            stcc.TeamRating = avgRating + (0.25 * matchMVPs);

            
            stcc.LastUpdated = DateTime.UtcNow;

            c.SeasonTeamCardCache.Update(stcc);
            c.SaveChanges();
        }
    }
}
