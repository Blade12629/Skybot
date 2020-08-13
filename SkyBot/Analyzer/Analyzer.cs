using System.Collections.Generic;
using System.Text;
using System.Linq;
using System;
using DSharpPlus.Entities;
using SkyBot.Analyzer.Enums;
using OsuHistoryEndPoint;
using SkyBot.Analyzer.Results;
using SkyBot.Database.Models.Statistics;
using System.Reflection;
using System.Globalization;
using OsuHistoryEndPoint.Data;
using System.Diagnostics;

namespace SkyBot.Analyzer
{
    public static class OsuAnalyzer
    {
        /// <summary>
        /// Creates a statistic for a osu mp match
        /// </summary>
        /// <param name="history">History file</param>
        /// <param name="guild">Discord guild</param>
        /// <param name="matchId">Match Id</param>
        /// <param name="warmupCount">Warmup Map Total Count</param>
        /// <param name="stage">Stage</param>
        /// <param name="submitStats">Submit stats to DB</param>
        /// <param name="beatmapsToIgnore">Ignore specific beatmaps</param>
        public static AnalyzerResult CreateStatistic(History history, DiscordGuild guild, int matchId, int warmupCount, string stage, bool submitStats, params long[] beatmapsToIgnore)
        {
            if (history == null)
                throw new ArgumentNullException(nameof(history));
            else if (guild == null)
                throw new ArgumentNullException(nameof(guild));

            string matchName = GetData.GetMatchNames(history)[0];

            List<HistoryGame> games = GetData.GetMatches(history);
            //beatmapid, score
            List<(long, HistoryScore)> scores = new List<(long, HistoryScore)>();

            int teamRedWins = 0;
            int teamBlueWins = 0;

            for (int i = 0; i < games.Count; i++)
            {
                if (games[i].Beatmap == null || string.IsNullOrEmpty(games[i].TeamType) ||
                    games[i].TeamType.Equals("head-to-head", StringComparison.CurrentCultureIgnoreCase))
                    continue;
                else if (beatmapsToIgnore != null && beatmapsToIgnore.Length > 0 && beatmapsToIgnore.Contains((long)(games[i].Beatmap.Id)))
                {
                    games.RemoveAt(i);
                    i--;
                    continue;
                }

                int teamRedWinsCurrent = 0;
                int teamBlueWinsCurrent = 0;
                for (int x = 0; x < games[i].Scores.Length ; x++)
                {
                    switch (games[i].Scores[x].Match.Team.ToLower(CultureInfo.CurrentCulture))
                    {
                        case "red":
                            teamRedWinsCurrent += games[i].Scores[x].Score;
                            break;

                        case "blue":
                            teamBlueWinsCurrent += games[i].Scores[x].Score;
                            break;

                        default:
                            break;
                    }

                    scores.Add((games[i].Beatmap.Id, games[i].Scores[x]));
                }

                if (teamRedWinsCurrent > teamBlueWinsCurrent)
                    teamRedWins++;
                else if (teamBlueWinsCurrent > teamRedWinsCurrent)
                    teamBlueWins++;
            }

            var HighestScoreRankingResult = CalculateHighestRankingAndPlayCount(games.ToArray(), history, warmupCount, beatmapsToIgnore);
                
            (string, string) teamNames = GetVersusTeamNames(matchName);

            TeamColor winningTeam;
            TeamColor losingTeam;
            if (teamBlueWins > teamRedWins)
            {
                winningTeam = TeamColor.Blue;
                losingTeam = TeamColor.Red;
            }
            else
            {
                winningTeam = TeamColor.Red;
                losingTeam = TeamColor.Blue;
            }

            AnalyzerResult result = new AnalyzerResult()
            {
                MatchId = matchId,
                MatchName = matchName,
                HighestScore = HighestScoreRankingResult.Item1[0].Item1,
                HighestScoreBeatmap = HighestScoreRankingResult.Item1[0].Item2,
                HighestScoresRanking = HighestScoreRankingResult.Item1[0].Item3,

                HighestAccuracyScore = HighestScoreRankingResult.Item1[1].Item1,
                HighestAccuracyBeatmap = HighestScoreRankingResult.Item1[1].Item2,
                HighestAverageAccuracyRanking = HighestScoreRankingResult.Item1[1].Item3,
                    
                WinningTeam = winningTeam == TeamColor.Red ? teamNames.Item2 : teamNames.Item1,
                WinningTeamColor = winningTeam,
                WinningTeamWins = winningTeam == TeamColor.Red ? teamRedWins : teamBlueWins,

                LosingTeam = losingTeam == TeamColor.Red ? teamNames.Item2 : teamNames.Item1,
                LosingTeamWins = losingTeam == TeamColor.Red ? teamRedWins : teamBlueWins,
                Scores = scores.ToArray(),

                Stage = stage,

                TimeStamp = history.Events.Last().TimeStamp
            };

            result.HighestScoreUser = HighestScoreRankingResult.Item1[0].Item3.FirstOrDefault(r => r.Player.UserId == result.HighestScore.UserId).Player;
            result.HighestAccuracyUser = HighestScoreRankingResult.Item1[1].Item3.FirstOrDefault(r => r.Player.UserId == result.HighestAccuracyScore.UserId).Player;
            result.Ranks = HighestScoreRankingResult.Item1[0].Item3;
            result.Beatmaps = HighestScoreRankingResult.Item2.Select(b => b.BeatMap).ToArray();

            if (submitStats)
            {
                SubmitStats(result, guild, games);

                using DBContext c = new DBContext();

                for (int i = 0; i < result.Ranks.Length; i++)
                    StatisticCache.ForceRefreshPlayerCache(result.Ranks[i].Player.UserId, c, (long)guild.Id, GetOverallRating);

                StatisticCache.ForceRefreshTeamCache(result.WinningTeam, c, (long)guild.Id);
                StatisticCache.ForceRefreshTeamCache(result.LosingTeam, c, (long)guild.Id);
            }

            return result;
        }

        /// <summary>
        /// Removes a match from the DB
        /// </summary>
        public static void RemoveMatch(long matchId, DiscordGuild guild)
        {
            using DBContext c = new DBContext();
            SeasonResult sr = c.SeasonResult.FirstOrDefault(sr => sr.MatchId == matchId &&
                                                                  sr.DiscordGuildId == (long)guild.Id);

            if (sr == null)
                return;

            c.SeasonScore.Where(sc => sc.SeasonResultId == sr.Id).ToList().ForEach(sr => c.SeasonScore.Remove(sr));

            c.SaveChanges();
        }

        public static DiscordEmbed GetMatchResultEmbed(long matchId)
        {
            using DBContext c = new DBContext();
            Func<int, bool> intToBool = new Func<int, bool>(i =>
            {
                if (i == 0)
                    return false;

                return true;
            });

            SeasonResult result = c.SeasonResult.FirstOrDefault(r => r.MatchId == matchId);

            if (result == null)
                return null;

            SeasonPlayer highestGpsWinningPlayer, highestGpsLosingPlayer;

            List<SeasonScore> scores = c.SeasonScore.Where(s => s.SeasonResultId == result.Id).ToList();

            SeasonScore highestAcc = scores.OrderByDescending(s => s.Accuracy).ElementAt(0);
            SeasonScore highestScore = scores.OrderByDescending(s => s.Score).ElementAt(0);

            SeasonBeatmap highestAccMap = c.SeasonBeatmap.FirstOrDefault(b => b.BeatmapId == highestAcc.BeatmapId);
            SeasonBeatmap highestScoreMap = c.SeasonBeatmap.FirstOrDefault(b => b.BeatmapId == highestScore.BeatmapId);

            SeasonPlayer highestAccPlayer = c.SeasonPlayer.FirstOrDefault(b => b.Id == highestAcc.SeasonPlayerId);
            SeasonPlayer highestScorePlayer = c.SeasonPlayer.FirstOrDefault(b => b.Id == highestScore.SeasonPlayerId);

            List<SeasonScore> highestGpsScores = scores.Where(s => s.HighestGeneralPerformanceScore).ToList();

            SeasonScore highestGpsWinningScore = highestGpsScores.FirstOrDefault(s => s.TeamName.Equals(result.WinningTeam, StringComparison.CurrentCultureIgnoreCase));
            SeasonScore highestGpsLosingScore = highestGpsScores.FirstOrDefault(s => s.TeamName.Equals(result.LosingTeam, StringComparison.CurrentCultureIgnoreCase));

            highestGpsWinningPlayer = c.SeasonPlayer.FirstOrDefault(b => b.Id == highestGpsWinningScore.SeasonPlayerId);
            highestGpsLosingPlayer = c.SeasonPlayer.FirstOrDefault(b => b.Id == highestGpsLosingScore.SeasonPlayerId);

            int rnd = Program.Random.Next(0, 8);
            DiscordColor color;
            switch (rnd)
            {
                default:
                case 0:
                    color = DiscordColor.Aquamarine;
                    break;
                case 1:
                    color = DiscordColor.Red;
                    break;
                case 2:
                    color = DiscordColor.Blurple;
                    break;
                case 3:
                    color = DiscordColor.Green;
                    break;
                case 4:
                    color = DiscordColor.Yellow;
                    break;
                case 5:
                    color = DiscordColor.Orange;
                    break;
                case 6:
                    color = DiscordColor.Black;
                    break;
                case 7:
                    color = DiscordColor.Gold;
                    break;
                case 8:
                    color = DiscordColor.CornflowerBlue;
                    break;
            }

            string description = string.Format(CultureInfo.CurrentCulture, ResourceStats.TeamWon, result.WinningTeam, result.WinningTeamWins, result.LosingTeamWins);

            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder()
            {
                Title = result.MatchName,
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"{ResourceStats.MatchPlayed} {result.TimeStamp}",
                },
                Color = color,
            };

            Dictionary<long, List<(double, double, double)>> playerValues = new Dictionary<long, List<(double, double, double)>>();

            for (int i = 0; i < scores.Count; i++)
            {
                if (!playerValues.ContainsKey(scores[i].SeasonPlayerId))
                {
                    playerValues.Add(scores[i].SeasonPlayerId, new List<(double, double, double)>() { (scores[i].Accuracy, scores[i].GeneralPerformanceScore, scores[i].Score) });
                    continue;
                }

                playerValues[scores[i].SeasonPlayerId].Add((scores[i].Accuracy, scores[i].GeneralPerformanceScore, scores[i].Score));
            }

            Dictionary<long, (double, double, double)> playerAverage = new Dictionary<long, (double, double, double)>();

            foreach (var pair in playerValues)
            {
                double avgAcc = pair.Value.Sum(s => s.Item1) / pair.Value.Count;
                double avgGps = pair.Value.Sum(s => s.Item2) / pair.Value.Count;
                double avgScore = pair.Value.Sum(s => s.Item3) / pair.Value.Count;

                playerAverage.Add(pair.Key, (avgAcc, avgGps, avgScore));
            }

            var sortedAvgAcc = playerAverage.OrderByDescending(p => p.Value.Item1).ToList();
            var sortedAvgGps = playerAverage.OrderByDescending(p => p.Value.Item2).ToList();
            var sortedAvgScore = playerAverage.OrderByDescending(p => p.Value.Item3).ToList();


            KeyValuePair<long, (double, double, double)> playerTeamAHighestAvgGps = sortedAvgGps.ElementAt(0);
            SeasonPlayer playerTeamAHighestGps = c.SeasonPlayer.FirstOrDefault(p => p.Id == playerTeamAHighestAvgGps.Key);
            KeyValuePair<long, (double, double, double)> playerTeamBHighestAvgGps = default;
            SeasonPlayer playerTeamBHighestGps = null;

            KeyValuePair<long, (double, double, double)> playerTeamAHighestAvgAcc = sortedAvgAcc.ElementAt(0);
            SeasonPlayer playerTeamAHighestAcc = c.SeasonPlayer.FirstOrDefault(p => p.Id == playerTeamAHighestAvgAcc.Key);
            KeyValuePair<long, (double, double, double)> playerTeamBHighestAvgAcc = default;
            SeasonPlayer playerTeamBHighestAcc = null;

            KeyValuePair<long, (double, double, double)> playerTeamAHighestAvgScore = sortedAvgScore.ElementAt(0);
            SeasonPlayer playerTeamAHighestScore = c.SeasonPlayer.FirstOrDefault(p => p.Id == playerTeamAHighestAvgScore.Key);
            KeyValuePair<long, (double, double, double)> playerTeamBHighestAvgScore = default;
            SeasonPlayer playerTeamBHighestScore = null;

            for (int i = 1; i < sortedAvgGps.Count; i++)
            {
                var pair = sortedAvgGps.ElementAt(i);
                var player = c.SeasonPlayer.FirstOrDefault(p => p.Id == pair.Key);

                if (player.TeamName.Equals(playerTeamAHighestGps.TeamName, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (playerTeamAHighestAvgGps.Value.Item2 >= pair.Value.Item2)
                        continue;

                    playerTeamAHighestAvgGps = pair;
                    playerTeamAHighestGps = player;
                }
                else
                {
                    if (playerTeamBHighestAvgGps.Equals(default))
                    {
                        playerTeamBHighestAvgGps = pair;
                        playerTeamBHighestGps = player;

                        continue;
                    }

                    if (playerTeamBHighestAvgGps.Value.Item2 >= pair.Value.Item2)
                        continue;

                    playerTeamBHighestAvgGps = pair;
                    playerTeamBHighestGps = player;
                }
            }

            for (int i = 1; i < sortedAvgScore.Count; i++)
            {
                var pair = sortedAvgGps.ElementAt(i);
                var player = c.SeasonPlayer.FirstOrDefault(p => p.Id == pair.Key);

                if (player.TeamName.Equals(playerTeamAHighestScore.TeamName, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (playerTeamAHighestAvgScore.Value.Item3 >= pair.Value.Item3)
                        continue;

                    playerTeamAHighestAvgScore = pair;
                    playerTeamAHighestScore = player;
                }
                else
                {
                    if (playerTeamBHighestAvgScore.Equals(default))
                    {
                        playerTeamBHighestAvgScore = pair;
                        playerTeamBHighestScore = player;

                        continue;
                    }

                    if (playerTeamBHighestAvgScore.Value.Item3 >= pair.Value.Item3)
                        continue;

                    playerTeamBHighestAvgScore = pair;
                    playerTeamBHighestScore = player;
                }
            }

            for (int i = 1; i < sortedAvgAcc.Count; i++)
            {
                var pair = sortedAvgGps.ElementAt(i);
                var player = c.SeasonPlayer.FirstOrDefault(p => p.Id == pair.Key);

                if (player.TeamName.Equals(playerTeamAHighestAcc.TeamName, StringComparison.CurrentCultureIgnoreCase))
                {
                    if (playerTeamAHighestAvgAcc.Value.Item1 >= pair.Value.Item1)
                        continue;

                    playerTeamAHighestAvgAcc = pair;
                    playerTeamAHighestAcc = player;
                }
                else
                {
                    if (playerTeamBHighestAvgAcc.Equals(default))
                    {
                        playerTeamBHighestAvgAcc = pair;
                        playerTeamBHighestAcc = player;

                        continue;
                    }

                    if (playerTeamBHighestAvgAcc.Value.Item1 >= pair.Value.Item1)
                        continue;

                    playerTeamBHighestAvgAcc = pair;
                    playerTeamBHighestAcc = player;
                }
            }

            discordEmbedBuilder.AddField(ResourceStats.HighestAccuracy, string.Format(CultureInfo.CurrentCulture, ResourceStats.HighestAccuracyMap,
                                                                                      highestAccPlayer.LastOsuUsername,
                                                                                      highestAccMap.Author,
                                                                                      highestAccMap.Title,
                                                                                      highestAccMap.Difficulty,
                                                                                      highestAccMap.DifficultyRating,
                                                                                      string.Format(CultureInfo.CurrentCulture, "{0:n0}", highestAcc.Score),
                                                                                      Math.Round(highestAcc.Accuracy, 2, MidpointRounding.AwayFromZero)));

            discordEmbedBuilder.AddField("——————————————————", result.WinningTeam, true);
            discordEmbedBuilder.AddField("——————————————————", result.LosingTeam, true);


            discordEmbedBuilder.AddField(".", ResourceStats.MVP);

            if (playerTeamAHighestGps.TeamName.Equals(highestGpsWinningPlayer.TeamName, StringComparison.CurrentCultureIgnoreCase))
            {
                discordEmbedBuilder.AddField($"{playerTeamAHighestGps.LastOsuUsername}: {Math.Round(playerTeamAHighestAvgGps.Value.Item2, 2, MidpointRounding.AwayFromZero)} {ResourceStats.GPS}", ".", true);
                discordEmbedBuilder.AddField($"{playerTeamBHighestGps.LastOsuUsername}: {Math.Round(playerTeamBHighestAvgGps.Value.Item2, 2, MidpointRounding.AwayFromZero)} {ResourceStats.GPS}", ".", true);
            }
            else
            {
                discordEmbedBuilder.AddField($"{playerTeamBHighestGps.LastOsuUsername}: {Math.Round(playerTeamBHighestAvgGps.Value.Item2, 2, MidpointRounding.AwayFromZero)} {ResourceStats.GPS}", ".", true);
                discordEmbedBuilder.AddField($"{playerTeamAHighestGps.LastOsuUsername}: {Math.Round(playerTeamAHighestAvgGps.Value.Item2, 2, MidpointRounding.AwayFromZero)} {ResourceStats.GPS}", ".", true);
            }

            discordEmbedBuilder.AddField(".", ResourceStats.HighestAvgScore);

            if (playerTeamAHighestScore.TeamName.Equals(highestGpsWinningPlayer.TeamName, StringComparison.CurrentCultureIgnoreCase))
            {
                discordEmbedBuilder.AddField($"{playerTeamAHighestScore.LastOsuUsername}: {string.Format(CultureInfo.CurrentCulture, "{0:n0}", (int)playerTeamAHighestAvgScore.Value.Item3)} {ResourceStats.Score}", ".", true);
                discordEmbedBuilder.AddField($"{playerTeamBHighestScore.LastOsuUsername}: {string.Format(CultureInfo.CurrentCulture, "{0:n0}", (int)playerTeamBHighestAvgScore.Value.Item3)} {ResourceStats.Score}", ".", true);
            }
            else
            {
                discordEmbedBuilder.AddField($"{playerTeamBHighestScore.LastOsuUsername}: {string.Format(CultureInfo.CurrentCulture, "{0:n0}", (int)playerTeamBHighestAvgScore.Value.Item3)} {ResourceStats.Score}", ".", true);
                discordEmbedBuilder.AddField($"{playerTeamAHighestScore.LastOsuUsername}: {string.Format(CultureInfo.CurrentCulture, "{0:n0}", (int)playerTeamAHighestAvgScore.Value.Item3)} {ResourceStats.Score}", ".", true);
            }

            discordEmbedBuilder.AddField(".", ResourceStats.HighestAvgAccuracy);

            if (playerTeamAHighestAcc.TeamName.Equals(highestGpsWinningPlayer.TeamName, StringComparison.CurrentCultureIgnoreCase))
            {
                discordEmbedBuilder.AddField($"{playerTeamAHighestAcc.LastOsuUsername}: {Math.Round(playerTeamAHighestAvgAcc.Value.Item1, 2, MidpointRounding.AwayFromZero)} %", "——————————————————", true);
                discordEmbedBuilder.AddField($"{playerTeamBHighestAcc.LastOsuUsername}: {Math.Round(playerTeamBHighestAvgAcc.Value.Item1, 2, MidpointRounding.AwayFromZero)} %", "——————————————————", true);
            }
            else
            {
                discordEmbedBuilder.AddField($"{playerTeamBHighestAcc.LastOsuUsername}: {Math.Round(playerTeamBHighestAvgAcc.Value.Item1, 2, MidpointRounding.AwayFromZero)} %", "——————————————————", true);
                discordEmbedBuilder.AddField($"{playerTeamAHighestAcc.LastOsuUsername}: {Math.Round(playerTeamAHighestAvgAcc.Value.Item1, 2, MidpointRounding.AwayFromZero)} %", "——————————————————", true);
            }

            return discordEmbedBuilder.Build();
        }

        public static void SubmitStats(AnalyzerResult ar, DiscordGuild guild, List<HistoryGame> games, params long[] beatmapsToIgnore)
        {
            if (ar == null)
                throw new ArgumentNullException(nameof(ar));
            else if (guild == null)
                throw new ArgumentNullException(nameof(guild));
            else if (games == null)
                throw new ArgumentNullException(nameof(games));

            using DBContext c = new DBContext();

            SeasonResult sr = c.SeasonResult.FirstOrDefault(sr => sr.MatchId == ar.MatchId && sr.DiscordGuildId == (long)guild.Id);

            if (sr != null)
            {
                Logger.Log("Failed to submit stats, match already exist", LogLevel.Error);
                return;
            }

            sr = new SeasonResult()
            {
                DiscordGuildId = (long)guild.Id,
                LosingTeam = ar.LosingTeam,
                LosingTeamWins = ar.LosingTeamWins,
                MatchId = ar.MatchId,
                MatchName = ar.MatchName,
                Stage = ar.Stage,
                TimeStamp = ar.TimeStamp,
                WinningTeam = ar.WinningTeam,
                WinningTeamColor = (int)ar.WinningTeamColor,
                WinningTeamWins = ar.WinningTeamWins,
            };

            sr = c.SeasonResult.Add(sr).Entity;
            c.SaveChanges();

            //Team blue, team red
            (string, string) versusNames = GetVersusTeamNames(ar.MatchName);

            for (int i = 0; i < ar.Beatmaps.Length; i++)
            {
                SeasonBeatmap sb = c.SeasonBeatmap.FirstOrDefault(sb => sb.BeatmapId == (long)ar.Beatmaps[i].Id);

                if (sb != null)
                    continue;

                sb = new SeasonBeatmap()
                {
                    Author = ar.Beatmaps[i].Beatmapset.Artist,
                    BeatmapId = (long)ar.Beatmaps[i].Id,
                    Difficulty = ar.Beatmaps[i].Version,
                    DifficultyRating = ar.Beatmaps[i].DifficultyRating,
                    Title = ar.Beatmaps[i].Beatmapset.Title
                };

                c.SeasonBeatmap.Add(sb);
            }
            c.SaveChanges();

            List<SeasonPlayer> players = new List<SeasonPlayer>();

            for (int i = 0; i < ar.Ranks.Length; i++)
            {
                Rank rank = ar.Ranks[i];
                Player player = rank.Player;

                SeasonPlayer sp = c.SeasonPlayer.FirstOrDefault(sp => sp.DiscordGuildId == (long)guild.Id && sp.OsuUserId == (long)player.UserId);

                if (sp != null)
                {
                    players.Add(sp);
                    continue;
                }

                sp = new SeasonPlayer()
                {
                    DiscordGuildId = (long)guild.Id,
                    LastOsuUsername = player.UserName,
                    OsuUserId = (long)player.UserId,
                    TeamName = player.HighestScore.Match.Team.Equals("red", StringComparison.CurrentCultureIgnoreCase) ? versusNames.Item2 : versusNames.Item1
                };

               players.Add(c.SeasonPlayer.Add(sp).Entity);
            }
            c.SaveChanges();

            List<SeasonScore> scores = new List<SeasonScore>();

            games = games.OrderBy(g => g.StartTime).ToList();

            int playOrder = 0;
            for (int i = 0; i < games.Count; i++)
            {
                var game = games[i];

                playOrder++;
                for (int x = 0; x < game.Scores.Length; x++)
                {
                    if (games[i].Beatmap == null || string.IsNullOrEmpty(games[i].TeamType) ||
                        games[i].TeamType.Equals("head-to-head", StringComparison.CurrentCultureIgnoreCase) ||
                        (beatmapsToIgnore != null && beatmapsToIgnore.Length > 0 && beatmapsToIgnore.Contains((long)(games[i].Beatmap.Id))))
                        continue;

                    var score = game.Scores[x];

                    SeasonPlayer sp = players.First(p => p.OsuUserId == (long)score.UserId);
                    SeasonScore ss = c.SeasonScore.FirstOrDefault(ss => ss.SeasonPlayerId == sp.Id &&
                                                                        ss.PlayedAt == (game.EndTime.HasValue ? game.EndTime.Value : game.StartTime));

                    if (ss != null)
                        continue;

                    ss = new SeasonScore()
                    {
                        Id = 0,
                        Accuracy = (float)score.Accuracy,
                        Score = score.Score,
                        SeasonPlayerId = sp.Id,
                        SeasonResultId = sr.Id,
                        Count100 = score.Statistics.Count100,
                        Count300 = score.Statistics.Count300,
                        Count50 = score.Statistics.Count50,
                        CountGeki = score.Statistics.CountGeki,
                        CountKatu = score.Statistics.CountKatu,
                        CountMiss = score.Statistics.CountMiss,
                        MaxCombo = score.MaxCombo,
                        Pass = score.Match.Pass,
                        Perfect = score.Perfect,
                        PlayedAt = game.EndTime.HasValue ? game.EndTime.Value : game.StartTime,
                        TeamName = sp.TeamName,
                        TeamVs = true,
                        PlayOrder = playOrder,
                        BeatmapId = (long)game.Beatmap.Id,
                    };

                    scores.Add(ss);
                }
            }

            CalculateGPS(ref scores);

            SeasonScore teamAHighestGPS = scores[0];
            SeasonScore teamBHighestGPS = null;
            for (int i = 1; i < scores.Count; i++)
            {
                if (teamAHighestGPS.TeamName.Equals(scores[i].TeamName, StringComparison.CurrentCulture))
                {
                    if (teamAHighestGPS.GeneralPerformanceScore < scores[i].GeneralPerformanceScore)
                        teamAHighestGPS = scores[i];
                }
                else if (teamBHighestGPS == null || teamBHighestGPS.GeneralPerformanceScore < scores[i].GeneralPerformanceScore)
                        teamBHighestGPS = scores[i];
            }

            teamAHighestGPS.HighestGeneralPerformanceScore = true;
            teamBHighestGPS.HighestGeneralPerformanceScore = true;

            c.SeasonScore.AddRange(scores);

            c.SaveChanges();
        }

        public static void UpdateCaches(DiscordGuild guild)
        {
            if (guild == null)
                throw new ArgumentNullException(nameof(guild));

            using DBContext c = new DBContext();
            List<SeasonPlayer> sp = c.SeasonPlayer.Where(sp => sp.DiscordGuildId == (long)guild.Id).ToList();

            for (int i = 0; i < sp.Count; i++)
                StatisticCache.ForceRefreshPlayerCache(sp[i].OsuUserId, c, (long)guild.Id, GetOverallRating);
        }

        private static void SetProperty(object instance, string propertyName, object newValue, StringComparison nameComparer = StringComparison.CurrentCultureIgnoreCase)
        {
            try
            {
                Type instanceType = instance.GetType();
                PropertyInfo[] properties = instanceType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                for (int i = 0; i < properties.Length; i++)
                {
                    if (!properties[i].Name.Equals(propertyName, nameComparer))
                        continue;

                    properties[i].SetValue(instance, newValue);
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString(), LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// Gets osu mp match team names
        /// </summary>
        private static (string, string) GetVersusTeamNames(string matchName)
        {
            string[] MatchNameSplit = matchName.Split(' ');
            string teamRed = MatchNameSplit[1].TrimStart('(');
            int teamVsIndex = MatchNameSplit.ToList().FindIndex(str => str.ToLower(CultureInfo.CurrentCulture).Equals("vs", StringComparison.CurrentCulture));

            for (int i = 2; i < teamVsIndex; i++)
                teamRed += string.Format(CultureInfo.CurrentCulture, " {0}", MatchNameSplit[i]);

            string teamBlue = MatchNameSplit[teamVsIndex + 1].TrimStart('(');
            teamRed = teamRed.TrimEnd(')');

            for (int i = teamVsIndex + 2; i < MatchNameSplit.Length; i++)
                teamBlue += string.Format(CultureInfo.CurrentCulture, " {0}", MatchNameSplit[i]);

            teamBlue = teamBlue.TrimEnd(')');

            return (teamRed, teamBlue);
        }

        /// <summary>
        /// calculates the highest ranking players and beatmap play counts
        /// </summary>
        /// <param name="games"><see cref="GetData.GetMatches(HistoryJson.History)"/></param>
        /// <returns>Tuple { Tuple { HighestScore, HighestScoreBeatmap, HighestScoreRanking }[], BeatmapPlayCount } }</returns>
        private static Tuple<Tuple<HistoryScore, HistoryBeatmap, Rank[]>[], BeatmapPlayCount[]> CalculateHighestRankingAndPlayCount(HistoryGame[] games, History history, int warmupCount, params long[] beatmapsToIgnore)
        {
            HistoryScore highestScore = null;
            HistoryBeatmap highestScoreBeatmap = null;
            List<Player> highestScoreRanking = new List<Player>();
            List<Rank> sortedRanksScore = new List<Rank>();

            HistoryScore highestAccuracy = null;
            HistoryBeatmap highestAccuracyBeatmap = null;
            List<Rank> sortedRanksAccuracy = new List<Rank>();

            StringComparer curCultIgnore = StringComparer.CurrentCultureIgnoreCase;

            List<HistoryScore> scores = new List<HistoryScore>();
            List<BeatmapPlayCount> playCounts = new List<BeatmapPlayCount>();

            int warmupCounter = 0;

            for (int i = 0; i < games.Length; i++)
            {
                HistoryGame game = games[i];

                if (games[i].Beatmap == null || string.IsNullOrEmpty(games[i].TeamType) ||
                    games[i].TeamType.Equals("head-to-head", StringComparison.CurrentCultureIgnoreCase) ||
                    (beatmapsToIgnore != null && beatmapsToIgnore.Length > 0 && beatmapsToIgnore.Contains((long)(games[i].Beatmap.Id))))
                    continue;

                if (game.Scores == null || game.Scores.Length == 0)
                    continue;
                else if (warmupCount > 0 && warmupCounter < warmupCount)
                {
                    warmupCounter++;
                    continue;
                }

                int playCountIndex = playCounts.FindIndex(bpc => bpc.BeatMap.Id == game.Beatmap.Id);

                if (playCountIndex > -1)
                    playCounts[playCountIndex].Count++;
                else
                    playCounts.Add(new BeatmapPlayCount()
                    {
                        BeatMap = game.Beatmap,
                        Count = 1,
                    });

                for (int x = 0; x < game.Scores.Length; x++)
                {
                    HistoryScore score = game.Scores[x];
                    Player CurrentPlayer = highestScoreRanking.Find(player => player.UserId == score.UserId);

                    if (CurrentPlayer == null)
                    {
                        CurrentPlayer = new Player();
                        CurrentPlayer.UserId = score.UserId;
                        CurrentPlayer.UserName = GetData.GetUser(score, history).Username;
                        CurrentPlayer.Scores = new HistoryScore[] { score };
                        highestScoreRanking.Add(CurrentPlayer);
                    }
                    else
                    {
                        List<HistoryScore> scoresPlayer = CurrentPlayer.Scores.ToList();
                        scoresPlayer.Add(score);
                        CurrentPlayer.Scores = scoresPlayer.ToArray();
                    }

                    if (highestScore == null || score.Score > highestScore.Score)
                    {
                        highestScore = score;
                        highestScoreBeatmap = game.Beatmap;
                    }

                    if (highestAccuracy == null || highestAccuracy.Accuracy < score.Accuracy)
                    {
                        highestAccuracy = score;
                        highestAccuracyBeatmap = game.Beatmap;
                    }
                }
            }

            highestScoreRanking.ForEach(ob =>
            {
                ob.CalculateAverageAccuracy();
                ob.GetHighestScore();
            });

            highestScoreRanking = highestScoreRanking.OrderByDescending(player => player.HighestScore.Score).ToList();

            for (int i = 0; i < highestScoreRanking.Count; i++)
            {
                Rank rank = new Rank()
                {
                    Player = highestScoreRanking[i],
                    Place = i + 1
                };

                sortedRanksScore.Add(rank);
            };

            sortedRanksAccuracy = sortedRanksScore.OrderByDescending(r => r.Player.AverageAccuracy).ToList();

            for (int i = 0; i < sortedRanksAccuracy.Count; i++)
                sortedRanksAccuracy[i].Place = i + 1;

            return new Tuple<Tuple<HistoryScore, HistoryBeatmap, Rank[]>[], BeatmapPlayCount[]>(
                new Tuple<HistoryScore, HistoryBeatmap, Rank[]>[]
            {
                new Tuple<HistoryScore, HistoryBeatmap, Rank[]>(highestScore, highestScoreBeatmap, sortedRanksScore.ToArray()),
                new Tuple<HistoryScore, HistoryBeatmap, Rank[]>(highestAccuracy, highestAccuracyBeatmap, sortedRanksAccuracy.ToArray()),
            },
            playCounts.ToArray());
        }

        private static double GetOverallRating(SeasonPlayer player, DBContext c)
        {
            List<SeasonScore> scores = c.SeasonScore.Where(s => s.SeasonPlayerId == player.Id).ToList();

            double result = 0;

            int n = scores.Count;
            float x, y, z, acc, gps, miss;
            double accMax = 0;
            double gpsMax = 0;

            for (int i = 0; i < scores.Count; i++)
            {
                SeasonScore score = scores[i];

                x = score.Accuracy;
                z = (float)score.CountMiss;

                acc = ((x + x) * x) / (x * 3.0f);
                miss = z * 10 / x * 3;

                accMax += acc - miss;
                Logger.Log("acc: " + acc + " | acc Max: " + accMax + " | acc calculated: " + (acc - miss));

                if (score.GeneralPerformanceScore > 0)
                {
                    y = (float)score.GeneralPerformanceScore;
                    gps = (y * y * y) / y;
                    gpsMax += gps - miss;
                    Logger.Log("GPS: " + gps + " | GPS Max: " + gpsMax + " | gps calculated: " + (gps - miss));
                }
            }
            double accAvg = 0;
            double gpsAvg = 0;

            if (accMax > 0)
                accAvg = accMax / n;

            if (gpsMax > 0)
                gpsAvg = gpsMax / n;

            if (accAvg != 0 && gpsAvg != 0)
            {
                double overallRating = ((gpsAvg * gpsAvg) * (accAvg * accAvg)) / (gpsAvg * accAvg) / 100 / 2.5;

                result = Math.Round(overallRating, 2, MidpointRounding.AwayFromZero);
            }

            Logger.Log("Rating: " + result);

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scores">Map Scores</param>
        /// <returns>bot_season_player_id, gps</returns>
        private static void CalculateGPS(ref List<SeasonScore> scores)
        {
            const double ACC_MULTI = 3.8;
            const double SCORE_MULTI = 1.2;
            const double COMBO_MULTI = 0.8;
            const double HITS300_MULTI = 1.0;
            const double MISSES_MULTI = 0.15;

            if (scores.Count == 0)
                throw new ArgumentOutOfRangeException(nameof(scores));

            Dictionary<long, List<double>> results = new Dictionary<long, List<double>>();

            List<SeasonScore> scoresByAcc = scores.OrderBy(s => s.Accuracy).ToList();
            List<SeasonScore> scoresByScore = scores.OrderBy(s => s.Score).ToList();
            List<SeasonScore> scoresByMisses = scores.OrderBy(s => s.CountMiss).ToList();
            List<SeasonScore> scoresByCombo = scores.OrderBy(s => s.MaxCombo).ToList();
            List<SeasonScore> scoresByHits300 = scores.OrderBy(s => s.Count300).ToList();

            Dictionary<long, List<double>> resultMisses = new Dictionary<long, List<double>>();

            Action<List<SeasonScore>, double, Func<SeasonScore, SeasonScore, bool>, Dictionary<long, List<double>>, bool> calculator = new Action<List<SeasonScore>, double, Func<SeasonScore, SeasonScore, bool>, Dictionary<long, List<double>>, bool>((scores, multi, equalityCheck, ranking, subtract) =>
            {
                int x;
                for (int i = scores.Count - 1; i > 0; i--)
                {
                    x = i + 1;

                    while ((x < scores.Count && equalityCheck(scores[i], scores[x])))
                        x++;

                    float value = (float)(x * multi);

                    if (subtract && value > 0)
                        value *= -1;

                    if (ranking.ContainsKey(scores[i].SeasonPlayerId))
                        ranking[scores[i].SeasonPlayerId].Add(value);
                    else
                        ranking.Add(scores[i].SeasonPlayerId, new List<double>() { value });
                }
            });

            calculator(scoresByAcc, ACC_MULTI, new Func<SeasonScore, SeasonScore, bool>((ss1, ss2) => ss1.Accuracy == ss2.Accuracy), results, false);
            calculator(scoresByScore, SCORE_MULTI, new Func<SeasonScore, SeasonScore, bool>((ss1, ss2) => ss1.Score == ss2.Score), results, false);
            calculator(scoresByCombo, COMBO_MULTI, new Func<SeasonScore, SeasonScore, bool>((ss1, ss2) => ss1.MaxCombo == ss2.MaxCombo), results, false);
            calculator(scoresByHits300, HITS300_MULTI, new Func<SeasonScore, SeasonScore, bool>((ss1, ss2) => ss1.Count300 == ss2.Count300), results, false);
            calculator(scoresByMisses, MISSES_MULTI, new Func<SeasonScore, SeasonScore, bool>((ss1, ss2) => false), resultMisses, false);

            Dictionary<long, double> averagePerformances = new Dictionary<long, double>();

            foreach(var pair in results)
            {
                double total = 0;
                for (int i = 0; i < pair.Value.Count; i++)
                    total += pair.Value[i];

                total /= pair.Value.Count;

                List<double> misses = resultMisses[pair.Key];

                double missTotal = 0;
                for (int i = 0; i < misses.Count; i++)
                    missTotal += misses[i];

                missTotal /= misses.Count;

                averagePerformances.Add(pair.Key, total - missTotal);
            }

            for (int i = 0; i < averagePerformances.Count; i++)
            {
                var kvp = averagePerformances.ElementAt(i);

                var score = scores.FirstOrDefault(s => s.SeasonPlayerId == kvp.Key);

                if (scores == null || scores.Count == 0 || kvp.Value <= 0)
                    Debugger.Break();

                score.GeneralPerformanceScore = kvp.Value;
            }
        }
    }
}
