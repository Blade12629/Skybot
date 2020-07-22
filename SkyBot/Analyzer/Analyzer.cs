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

namespace SkyBot.Analyzer
{
    public static class Analyzer
    {
        private const float _accMulti = 0.8f;
        private const float _scoreMulti = 0.9f;
        private const float _missesMulti = 0.1f;
        private const float _comboMulti = 1.0f;
        private const float _300Multi = 1.0f;

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
        public static AnalyzerResult CreateStatistic(HistoryJson.History history, DiscordGuild guild, int matchId, int warmupCount, string stage, bool submitStats, params long[] beatmapsToIgnore)
        {
            string matchName = history.Events.FirstOrDefault(ob => ob.Detail.Type == "other").Detail.MatchName;

            List<HistoryJson.Game> games = GetData.GetMatches(history).ToList();
            //beatmapid, score
            List<(long, HistoryJson.Score)> scores = new List<(long, HistoryJson.Score)>();

            for (int i = 0; i < games.Count; i++)
            {
                if (beatmapsToIgnore.Contains((long)(games[i].beatmap.id ?? 0)) || games[i].team_type.Equals("head-to-head", StringComparison.CurrentCultureIgnoreCase))
                {
                    games.RemoveAt(i);
                    i--;
                    continue;
                }

                for (int x = 0; x < games[i].scores.Count; x++)
                    scores.Add((games[i].beatmap.id ?? -1, games[i].scores[x]));
            }

            var HighestScoreRankingResult = CalculateHighestRankingAndPlayCount(games.ToArray(), history, warmupCount, true);
                
            (string, string) teamNames = GetVersusTeamNames(matchName);
            Tuple<int, int> wins = GetWins(games.ToArray(), warmupCount);

            TeamColor winningTeam = wins.Item1 > wins.Item2 ? TeamColor.Blue : TeamColor.Red;
            TeamColor losingTeam = wins.Item1 > wins.Item2 ? TeamColor.Red : TeamColor.Blue;

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
                WinningTeamWins = winningTeam == TeamColor.Red ? wins.Item2 : wins.Item1,

                LosingTeam = losingTeam == TeamColor.Red ? teamNames.Item2 : teamNames.Item1,
                LosingTeamWins = losingTeam == TeamColor.Red ? wins.Item2 : wins.Item1,
                Scores = scores.ToArray(),

                Stage = stage,

                TimeStamp = history.Events.Last().TimeStamp
            };

            result.HighestScoreUser = HighestScoreRankingResult.Item1[0].Item3.FirstOrDefault(r => r.Player.UserId == result.HighestScore.user_id).Player;
            result.HighestAccuracyUser = HighestScoreRankingResult.Item1[1].Item3.FirstOrDefault(r => r.Player.UserId == result.HighestAccuracyScore.user_id).Player;
            result.Ranks = HighestScoreRankingResult.Item1[0].Item3;
            result.Beatmaps = HighestScoreRankingResult.Item2.Select(b => b.BeatMap).ToArray();

            if (submitStats)
            {
                SubmitStats(result, guild, games);

                using DBContext c = new DBContext();

                for (int i = 0; i < result.Ranks.Length; i++)
                    Cache.ForceRefreshPlayerCache(result.Ranks[i].Player.UserId, c, (long)guild.Id, GetOverallRating);

                Cache.ForceRefreshTeamCache(result.WinningTeam, c, (long)guild.Id);
                Cache.ForceRefreshTeamCache(result.LosingTeam, c, (long)guild.Id);
            }

            return result;
        }

        /// <summary>
        /// Gets a <see cref="HistoryJson.History"/> from the <paramref name="matchUrl"/>
        /// </summary>
        public static HistoryJson.History GetHistory(string matchUrl)
        {
            List<HistoryJson.History> histories = new List<HistoryJson.History>()
            {
                ParseMatch(matchUrl).Item1
            };

            HistoryJson.Event firstEvent = histories[0].Events[0];

            while(firstEvent.Detail == null || !firstEvent.Detail.Type.Equals("match-created", StringComparison.CurrentCultureIgnoreCase))
            {
                long before = firstEvent.EventId.Value;

                histories.Insert(0, ParseMatch(matchUrl, $"before={before}").Item1);
                firstEvent = histories[0].Events[0];
            }

            List<HistoryJson.Event> events = new List<HistoryJson.Event>();
            List<HistoryJson.User> users = new List<HistoryJson.User>();

            for (int i = 0; i < histories.Count; i++)
            {
                events.AddRange(histories[i].Events);
                users.AddRange(histories[i].Users);
            }

            HistoryJson.History history = new HistoryJson.History();

            SetProperty(history, "Events", events.ToArray());
            SetProperty(history, "Users", users.ToArray());
            SetProperty(history, "EventCount", events.Count);
            SetProperty(history, "LatestEventId", histories[histories.Count - 1].LatestEventId);
            SetProperty(histories, "current_game_id", histories[0].current_game_id ?? 0);

            return history;
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

        /// <summary>
        /// parses a osu mp match
        /// </summary>
        public static (HistoryJson.History, int) ParseMatch(string matchIdString, params string[] parameters)
        {
            const string historyUrl = "https://osu.ppy.sh/community/matches/";
            const string historyUrlVariant = "https://osu.ppy.sh/mp/";

            if (matchIdString.Contains(historyUrlVariant))
                matchIdString = matchIdString.Replace(historyUrlVariant, historyUrl);

            int matchId = -1;
            string[] multiLinkSplit = matchIdString.Split(new[] { "\r\n", "\r", "\n", Environment.NewLine }, StringSplitOptions.None);

            foreach (string s in multiLinkSplit)
            {
                if (string.IsNullOrEmpty(s))
                    continue;
                if (s.Contains(historyUrl))
                {
                    string[] split = s.Split('/');

                    foreach (string str in split)
                        if (int.TryParse(str, out int PresultSplit))
                        {
                            matchId = PresultSplit;
                            break;
                        }
                    if (matchId > 0)
                        break;

                    string numbers = null;
                    int indexOf = s.IndexOf(historyUrl) + historyUrl.Length;

                    string sub = s.Substring(indexOf, s.Length - indexOf);

                    foreach (char c in sub)
                    {
                        if (c.Equals(' '))
                            break;

                        if (int.TryParse(c.ToString(), out int result))
                            numbers += result;
                    }

                    if (int.TryParse(numbers, out int resultMP))
                        matchId = resultMP;
                }
            }

            if (matchId <= 0)
                return (null, -1);

            string endpointUrl = $"https://osu.ppy.sh/community/matches/{matchId}/history";

            if (parameters != null && parameters.Length > 0)
            {
                endpointUrl += "?" + parameters[0];

                for (int i = 1; i < parameters.Length; i++)
                    endpointUrl += "&" + parameters[i];
            }

            HistoryJson.History historyJson = GetData.ParseJsonFromUrl(endpointUrl);

            return (historyJson, matchId);
        }

        public static DiscordEmbed CreateStatisticEmbed(AnalyzerResult ar, DiscordColor embedColor)
        {
            string description = string.Format("Team {0} won! ({1}:{2})", ar.WinningTeam, ar.WinningTeamWins, ar.LosingTeamWins);

            DiscordEmbedBuilder discordEmbedBuilder = new DiscordEmbedBuilder()
            {
                Title = ar.MatchName,
                Description = description,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"Match played at {ar.TimeStamp}",
                },
                Color = embedColor,
            };

            var playersBlue = ar.HighestScoresRanking.Where(r => r.Player.Scores.ElementAt(r.Player.Scores.Length - 1).match.team.Trim(' ').Equals("blue", StringComparison.CurrentCultureIgnoreCase)).OrderByDescending(f => f.Player.MVPScore).ToList();
            var playersRed = ar.HighestScoresRanking.Where(r => r.Player.Scores.ElementAt(r.Player.Scores.Length - 1).match.team.Trim(' ').Equals("red", StringComparison.CurrentCultureIgnoreCase)).OrderByDescending(f => f.Player.MVPScore).ToList();

            var playerBlue = playersBlue.ElementAt(0).Player;
            var playerRed = playersRed.ElementAt(0).Player;
            //generated performance score = gps
            discordEmbedBuilder.AddField("Most Valuable Player", $"Team Blue: {playerBlue.UserName} ({playerBlue.MVPScore} GPS){Environment.NewLine}Team Red: {playerRed.UserName} ({playerRed.MVPScore} GPS)");

            discordEmbedBuilder.AddField("Highest Score", string.Format("{0} on the map {1} - {2} [{3}] ({4}*) with {5:n0} Points and {6}% Accuracy!",
                ar.HighestScoreUser.UserName, ar.HighestScoreBeatmap.beatmapset.artist,
                ar.HighestScoreBeatmap.beatmapset.title, ar.HighestScoreBeatmap.version,
                ar.HighestScoreBeatmap.difficulty_rating,
                string.Format("{0:n0}", ar.HighestScoreUser.HighestScore.score),
                Math.Round(ar.HighestScoreUser.HighestScore.accuracy.Value * 100.0f, 2, MidpointRounding.AwayFromZero)));

            discordEmbedBuilder.AddField("Highest Accuracy", string.Format("{0} on the map {1} - {2} [{3}] ({4}*) with {5:n0} Points and {6}% Accuracy!",
                ar.HighestAccuracyUser.UserName,
                ar.HighestAccuracyBeatmap.beatmapset.artist,
                ar.HighestAccuracyBeatmap.beatmapset.title,
                ar.HighestAccuracyBeatmap.version,
                ar.HighestAccuracyBeatmap.difficulty_rating,
                string.Format("{0:n0}", ar.HighestAccuracyScore.score),
                Math.Round(ar.HighestAccuracyScore.accuracy.Value * 100.0f, 2, MidpointRounding.AwayFromZero)));

            for (int i = 1; i < 4; i++)
            {
                Rank place = ar.HighestAverageAccuracyRanking.Last(ob => ob.Place == i) as Rank;
                (string, string) placeString = GetPlaceString(place);
                discordEmbedBuilder.AddField(placeString.Item1, placeString.Item2);
            }
            
            return discordEmbedBuilder.Build();

            (string, string) GetPlaceString(Rank place)
            {
                switch (place.Place)
                {
                    case 1:
                        return ("First Place", $"{ place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                    case 2:
                        return ("Second Place", $"{ place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                    case 3:
                        return ("Third Place", $"{place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                    //Normally unused
                    case 4:
                        return ("Fourth Place", $"{place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                    default:
                        return ($"{place.Place} Place", $"{ place.Player.UserName}: Average Acc: { place.Player.AverageAccuracyRounded}%");
                }
            }
        }

        public static void SubmitStats(AnalyzerResult ar, DiscordGuild guild, List<HistoryJson.Game> games)
        {
            using DBContext c = new DBContext();

            SeasonResult sr = c.SeasonResult.FirstOrDefault(sr => sr.MatchId == ar.MatchId);

            if (sr != null)
                return;

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
                SeasonBeatmap sb = c.SeasonBeatmap.FirstOrDefault(sb => sb.BeatmapId == (long)ar.Beatmaps[i].id);

                if (sb != null)
                    continue;

                sb = new SeasonBeatmap()
                {
                    Author = ar.Beatmaps[i].beatmapset.artist,
                    BeatmapId = (long)ar.Beatmaps[i].id,
                    Difficulty = ar.Beatmaps[i].version,
                    DifficultyRating = ar.Beatmaps[i].difficulty_rating,
                    Title = ar.Beatmaps[i].beatmapset.title
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
                    TeamName = player.HighestScore.match.team.Equals("red", StringComparison.CurrentCultureIgnoreCase) ? versusNames.Item2 : versusNames.Item1
                };

               players.Add(c.SeasonPlayer.Add(sp).Entity);
            }
            c.SaveChanges();

            List<SeasonScore> scores = new List<SeasonScore>();

            games = games.OrderBy(g => g.start_time).ToList();

            int playOrder = 0;
            for (int i = 0; i < games.Count; i++)
            {
                var game = games[i];

                playOrder++;
                for (int x = 0; x < game.scores.Count; x++)
                {
                    var score = game.scores[x];

                    SeasonPlayer sp = players.First(p => p.OsuUserId == (long)score.user_id.Value);
                    SeasonScore ss = c.SeasonScore.FirstOrDefault(ss => ss.SeasonPlayerId == sp.Id &&
                                                                        ss.PlayedAt == score.created_at.Value);

                    if (ss != null)
                        continue;

                    ss = new SeasonScore()
                    {
                        Id = 0,
                        Accuracy = score.accuracy.Value,
                        Score = score.score.Value,
                        SeasonPlayerId = sp.Id,
                        SeasonResultId = sr.Id,
                        Count100 = score.statistics.count_100.Value,
                        Count300 = score.statistics.count_300.Value,
                        Count50 = score.statistics.count_50.Value,
                        CountGeki = score.statistics.count_geki.Value,
                        CountKatu = score.statistics.count_katu.Value,
                        CountMiss = score.statistics.count_miss.Value,
                        MaxCombo = score.max_combo.Value,
                        Pass = score.match.pass.Value,
                        Perfect = score.perfect.Value,
                        PlayedAt = score.created_at.Value,
                        TeamName = sp.TeamName,
                        TeamVs = true,
                        PlayOrder = playOrder,
                        BeatmapId = (long)game.beatmap.id.Value,
                    };

                    scores.Add(ss);
                }
            }

            CalculateGPS(ref scores);

            double max = scores.Max(s => s.GeneralPerformanceScore);
            scores.First(s => s.GeneralPerformanceScore == max).HighestGeneralPerformanceScore = true;

            c.SeasonScore.AddRange(scores);

            c.SaveChanges();
        }

        public static void UpdateCaches(DiscordGuild guild)
        {
            using DBContext c = new DBContext();
            List<SeasonPlayer> sp = c.SeasonPlayer.Where(sp => sp.DiscordGuildId == (long)guild.Id).ToList();

            for (int i = 0; i < sp.Count; i++)
                Cache.ForceRefreshPlayerCache(sp[i].OsuUserId, c, (long)guild.Id, GetOverallRating);
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
                throw ex;
            }
        }

        /// <summary>
        /// Gets osu mp match team names
        /// </summary>
        private static (string, string) GetVersusTeamNames(string matchName)
        {
            string[] MatchNameSplit = matchName.Split(' ');
            string teamRed = MatchNameSplit[1].TrimStart('(');
            int teamVsIndex = MatchNameSplit.ToList().FindIndex(str => str.ToLower().Equals("vs"));

            for (int i = 2; i < teamVsIndex; i++)
                teamRed += string.Format(" {0}", MatchNameSplit[i]);

            string teamBlue = MatchNameSplit[teamVsIndex + 1].TrimStart('(');
            teamRed = teamRed.TrimEnd(')');

            for (int i = teamVsIndex + 2; i < MatchNameSplit.Count(); i++)
                teamBlue += string.Format(" {0}", MatchNameSplit[i]);

            teamBlue = teamBlue.TrimEnd(')');

            return (teamRed, teamBlue);
        }

        /// <summary>
        /// calculates the highest ranking players and beatmap play counts
        /// </summary>
        /// <param name="games"><see cref="GetData.GetMatches(HistoryJson.History)"/></param>
        /// <returns>Tuple { Tuple { HighestScore, HighestScoreBeatmap, HighestScoreRanking }[], BeatmapPlayCount } }</returns>
        private static Tuple<Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>[], BeatmapPlayCount[]> CalculateHighestRankingAndPlayCount(HistoryJson.Game[] games, HistoryJson.History history, int warmupCount, bool calculateMVP = false, params long[] beatmapsToIgnore)
        {
            HistoryJson.Score highestScore = null;
            HistoryJson.BeatMap highestScoreBeatmap = null;
            List<Player> highestScoreRanking = new List<Player>();
            List<Rank> sortedRanksScore = new List<Rank>();

            HistoryJson.Score highestAccuracy = null;
            HistoryJson.BeatMap highestAccuracyBeatmap = null;
            List<Rank> sortedRanksAccuracy = new List<Rank>();

            StringComparer curCultIgnore = StringComparer.CurrentCultureIgnoreCase;

            List<HistoryJson.Score> scores = new List<HistoryJson.Score>();
            List<BeatmapPlayCount> playCounts = new List<BeatmapPlayCount>();

            int warmupCounter = 0;

            for (int i = 0; i < games.Length; i++)
            {
                HistoryJson.Game game = games[i];

                if (beatmapsToIgnore.Contains((long)(game.beatmap.id ?? 0)) || game.team_type.Equals("head-to-head", StringComparison.CurrentCultureIgnoreCase))
                    continue;

                List<HistoryJson.Score> gameScores = game.scores;

                if (gameScores == null)
                    continue;
                else if (warmupCount > 0 && warmupCounter < warmupCount)
                {
                    warmupCounter++;
                    continue;
                }

                int playCountIndex = playCounts.FindIndex(bpc => bpc.BeatMap.id.Value == game.beatmap.id.Value);

                if (playCountIndex > -1)
                    playCounts[playCountIndex].Count++;
                else
                    playCounts.Add(new BeatmapPlayCount()
                    {
                        BeatMap = game.beatmap,
                        Count = 1,
                    });

                for (int x = 0; x < game.scores.Count; x++)
                {
                    HistoryJson.Score score = game.scores[x];
                    Player CurrentPlayer = highestScoreRanking.Find(player => player.UserId == score.user_id.Value);

                    if (CurrentPlayer == null)
                    {
                        CurrentPlayer = new Player();
                        CurrentPlayer.UserId = score.user_id.Value;
                        CurrentPlayer.UserName = GetData.GetUser(score, history).Username;
                        CurrentPlayer.Scores = new HistoryJson.Score[] { score };
                        highestScoreRanking.Add(CurrentPlayer);
                    }
                    else
                    {
                        List<HistoryJson.Score> scoresPlayer = CurrentPlayer.Scores.ToList();
                        scoresPlayer.Add(score);
                        CurrentPlayer.Scores = scoresPlayer.ToArray();
                    }

                    if (highestScore == null || score.score.Value > highestScore.score.Value)
                    {
                        highestScore = score;
                        highestScoreBeatmap = game.beatmap;
                    }

                    if (highestAccuracy == null || highestAccuracy.accuracy.Value < score.accuracy.Value)
                    {
                        highestAccuracy = score;
                        highestAccuracyBeatmap = game.beatmap;
                    }
                }

                if (calculateMVP)
                    CalculateMVP(ref highestScoreRanking, ref game);
            }

            highestScoreRanking.ForEach(ob =>
            {
                ob.CalculateAverageAccuracy();
                ob.GetHighestScore();
            });

            highestScoreRanking = highestScoreRanking.OrderByDescending(player => player.HighestScore.score.Value).ToList();

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

            return new Tuple<Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>[], BeatmapPlayCount[]>(
                new Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>[]
            {
                new Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>(highestScore, highestScoreBeatmap, sortedRanksScore.ToArray()),
                new Tuple<HistoryJson.Score, HistoryJson.BeatMap, Rank[]>(highestAccuracy, highestAccuracyBeatmap, sortedRanksAccuracy.ToArray()),
            },
            playCounts.ToArray());
        }

        private static void CalculateMVP(ref List<Player> highestScoreRanking, ref HistoryJson.Game game)
        {

            List<HistoryJson.Score> hAcc = game.scores.OrderBy(f => f.accuracy.Value).ToList();
            List<HistoryJson.Score> hScore = game.scores.OrderBy(f => f.score.Value).ToList();
            List<HistoryJson.Score> hMisses = game.scores.OrderByDescending(f => f.statistics.count_miss.Value).ToList();
            List<HistoryJson.Score> hCombo = game.scores.OrderBy(f => f.max_combo.Value).ToList();
            List<HistoryJson.Score> h300 = game.scores.OrderBy(f => f.statistics.count_300.Value).ToList();

            int x;
            for (int i = hAcc.Count - 1; i > 0; i--)
            {
                if (i < hAcc.Count - 1)
                {
                    x = i + 1;
                    while (x < hAcc.Count - 1 && hAcc[i].accuracy.Value == hAcc[x].accuracy.Value)
                        x++;

                    highestScoreRanking.Find(p => p.UserId == hAcc[i].user_id.Value).MVPScore += x * _accMulti;
                    continue;
                }

                highestScoreRanking.Find(p => p.UserId == hAcc[i].user_id.Value).MVPScore += i * _accMulti;
            }

            for (int i = hScore.Count - 1; i > 0; i--)
            {
                if (i < hScore.Count - 1)
                {
                    x = i + 1;
                    while (x < hScore.Count - 1 && hScore[i].score.Value == hScore[x].score.Value)
                        x++;

                    highestScoreRanking.Find(p => p.UserId == hScore[i].user_id.Value).MVPScore += x * _scoreMulti;
                    continue;
                }

                highestScoreRanking.Find(p => p.UserId == hScore[i].user_id.Value).MVPScore += i * _scoreMulti;
            }

            for (int i = hMisses.Count - 1; i > 0; i--)
            {
                if (i < hMisses.Count - 1)
                {
                    x = i + 1;
                    while (x < hMisses.Count - 1 && hMisses[i].statistics.count_miss.Value == hMisses[x].statistics.count_miss.Value)
                        x++;

                    highestScoreRanking.Find(p => p.UserId == hMisses[i].user_id.Value).MVPScore -= x * _missesMulti;
                    continue;
                }

                highestScoreRanking.Find(p => p.UserId == hMisses[i].user_id.Value).MVPScore -= i * _missesMulti;
            }

            for (int i = hCombo.Count - 1; i > 0; i--)
            {
                if (i < hCombo.Count - 1)
                {
                    x = i + 1;
                    while (x < hCombo.Count - 1 && hCombo[i].max_combo.Value == hCombo[x].max_combo.Value)
                        x++;

                    highestScoreRanking.Find(p => p.UserId == hCombo[i].user_id.Value).MVPScore += x * _comboMulti;
                    continue;
                }

                highestScoreRanking.Find(p => p.UserId == hCombo[i].user_id.Value).MVPScore += i * _comboMulti;
            }

            for (int i = h300.Count - 1; i > 0; i--)
            {
                if (i < h300.Count - 1)
                {
                    x = i + 1;
                    while (x < h300.Count - 1 && h300[i].max_combo.Value == h300[x].max_combo.Value)
                        x++;

                    highestScoreRanking.Find(p => p.UserId == h300[i].user_id.Value).MVPScore += x * _300Multi;
                    continue;
                }

                highestScoreRanking.Find(p => p.UserId == h300[i].user_id.Value).MVPScore += i * _300Multi;
            }
        }

        private static double GetOverallRating(SeasonPlayer player, DBContext c)
        {
            List<SeasonScore> scores = c.SeasonScore.Where(s => s.SeasonPlayerId == player.Id).ToList();

            double result = 0;

            List<double> gpsValues = new List<double>();
            List<double> accValues = new List<double>();

            int n = scores.Count;
            float x, y, z, acc, gps, miss;
            double accMax = 0;
            double gpsMax = 0;

            for (int i = 0; i < scores.Count; i++)
            {
                SeasonScore score = scores[i];

                x = score.Accuracy;
                y = (float)score.GeneralPerformanceScore;
                z = (float)score.CountMiss;

                if (x <= 0 || y <= 0)
                {
                    continue;
                }

                acc = ((x + x) * x) / (x * 3.0f);
                gps = (y * y * y) / (y * 0.5f);
                miss = z * 10 / x * 3;

                accMax += acc - miss;
                gpsMax += gps - miss;

                accValues.Add(acc);
                gpsValues.Add(gps);
            }
            double accAvg = 0;
            double gpsAvg = 0;

            if (accMax > 0)
            {
                accAvg = accMax / n;
                gpsAvg = gpsMax / n;
            }

            if (accAvg != 0 || gpsAvg != 0)
            {
                double overallRating = ((gpsAvg * gpsAvg) * (accAvg * accAvg)) / (gpsAvg * accAvg) / 100 / 30 / 2.5;

                result = Math.Round(overallRating, 2, MidpointRounding.AwayFromZero);
            }

            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scores">Map Scores</param>
        /// <returns>bot_season_player_id, gps</returns>
        private static void CalculateGPS(ref List<SeasonScore> scores)
        {
            List<SeasonScore> scoresByAcc = scores.OrderBy(s => s.Accuracy).ToList();
            List<SeasonScore> scoresByScore = scores.OrderBy(s => s.Score).ToList();
            List<SeasonScore> scoresByMisses = scores.OrderBy(s => s.CountMiss).ToList();
            List<SeasonScore> scoresByCombo = scores.OrderBy(s => s.MaxCombo).ToList();
            List<SeasonScore> scoresByHits300 = scores.OrderBy(s => s.Count300).ToList();

            const double SCORE_MULTI = 1.9;
            const double ACC_MULTI = 2.4;
            const double COMBO_MULTI = 2.0;
            const double HITS300_MULTI = 1.65;

            const double MISSES_MULTI = 1.15;
            const double MISSES_MULTI2 = 2.0;

            Dictionary<long, double> resultAcc = new Dictionary<long, double>();
            Dictionary<long, double> resultScore = new Dictionary<long, double>();
            Dictionary<long, double> resultMisses = new Dictionary<long, double>();
            Dictionary<long, double> resultCombo = new Dictionary<long, double>();
            Dictionary<long, double> resultHits300 = new Dictionary<long, double>();

            int x;
            for (int i = scoresByAcc.Count - 1; i >= 0; i--)
            {
                if (i < scoresByAcc.Count - 1)
                {
                    x = i + 1;
                    while (x < scoresByAcc.Count - 1 && scoresByAcc[i].Accuracy == scoresByAcc[x].Accuracy)
                        x++;

                    if (resultAcc.ContainsKey(scoresByAcc[i].SeasonPlayerId))
                        resultAcc[scoresByAcc[i].SeasonPlayerId] += x * ACC_MULTI;
                    else
                        resultAcc.Add(scoresByAcc[i].SeasonPlayerId, x * ACC_MULTI);

                    continue;
                }

                if (resultAcc.ContainsKey(scoresByAcc[i].SeasonPlayerId))
                    resultAcc[scoresByAcc[i].SeasonPlayerId] += i * ACC_MULTI;
                else
                    resultAcc.Add(scoresByAcc[i].SeasonPlayerId, i * ACC_MULTI);
            }

            for (int i = scoresByScore.Count - 1; i >= 0; i--)
            {
                if (i < scoresByScore.Count - 1)
                {
                    x = i + 1;
                    while (x < scoresByScore.Count - 1 && scoresByScore[i].Score == scoresByScore[x].Score)
                        x++;

                    if (resultScore.ContainsKey(scoresByScore[i].SeasonPlayerId))
                        resultScore[scoresByScore[i].SeasonPlayerId] += x * SCORE_MULTI;
                    else
                        resultScore.Add(scoresByScore[i].SeasonPlayerId, x * SCORE_MULTI);

                    continue;
                }

                if (resultScore.ContainsKey(scoresByScore[i].SeasonPlayerId))
                    resultScore[scoresByScore[i].SeasonPlayerId] += i * SCORE_MULTI;
                else
                    resultScore.Add(scoresByScore[i].SeasonPlayerId, i * SCORE_MULTI);
            }

            for (int i = scoresByMisses.Count - 1; i >= 0; i--)
            {
                if (i < scoresByMisses.Count - 1)
                {
                    x = i + 1;
                    while (x < scoresByMisses.Count - 1 && scoresByMisses[i].CountMiss == scoresByMisses[x].CountMiss)
                        x++;

                    if (resultMisses.ContainsKey(scoresByMisses[i].SeasonPlayerId))
                        resultMisses[scoresByMisses[i].SeasonPlayerId] += x * MISSES_MULTI * MISSES_MULTI2;
                    else
                        resultMisses.Add(scoresByMisses[i].SeasonPlayerId, x * MISSES_MULTI * MISSES_MULTI2);

                    continue;
                }

                if (resultMisses.ContainsKey(scoresByMisses[i].SeasonPlayerId))
                    resultMisses[scoresByMisses[i].SeasonPlayerId] += i * MISSES_MULTI * MISSES_MULTI2;
                else
                    resultMisses.Add(scoresByMisses[i].SeasonPlayerId, i * MISSES_MULTI * MISSES_MULTI2);
            }

            for (int i = scoresByCombo.Count - 1; i >= 0; i--)
            {
                if (i < scoresByCombo.Count - 1)
                {
                    x = i + 1;
                    while (x < scoresByCombo.Count - 1 && scoresByCombo[i].MaxCombo == scoresByCombo[x].MaxCombo)
                        x++;

                    if (resultCombo.ContainsKey(scoresByCombo[i].SeasonPlayerId))
                        resultCombo[scoresByCombo[i].SeasonPlayerId] += x * COMBO_MULTI;
                    else
                        resultCombo.Add(scoresByCombo[i].SeasonPlayerId, x * COMBO_MULTI);

                    continue;
                }

                if (resultCombo.ContainsKey(scoresByCombo[i].SeasonPlayerId))
                    resultCombo[scoresByCombo[i].SeasonPlayerId] += i * COMBO_MULTI;
                else
                    resultCombo.Add(scoresByCombo[i].SeasonPlayerId, i * COMBO_MULTI);

            }

            for (int i = scoresByHits300.Count - 1; i >= 0; i--)
            {
                if (i < scoresByHits300.Count - 1)
                {
                    x = i + 1;
                    while (x < scoresByHits300.Count - 1 && scoresByHits300[i].Count300 == scoresByHits300[x].Count300)
                        x++;

                    if (resultHits300.ContainsKey(scoresByHits300[i].SeasonPlayerId))
                        resultHits300[scoresByHits300[i].SeasonPlayerId] += x * HITS300_MULTI;
                    else
                        resultHits300.Add(scoresByHits300[i].SeasonPlayerId, x * HITS300_MULTI);

                    continue;
                }

                if (resultHits300.ContainsKey(scoresByHits300[i].SeasonPlayerId))
                    resultHits300[scoresByHits300[i].SeasonPlayerId] += i * HITS300_MULTI;
                else
                    resultHits300.Add(scoresByHits300[i].SeasonPlayerId, i * HITS300_MULTI);
            }

            for (int i = 0; i < resultAcc.Count; i++)
            {
                var pairAcc = resultAcc.ElementAt(i);
                var pairScore = resultScore.ElementAt(i);
                var pairCombo = resultCombo.ElementAt(i);
                var pairHits300 = resultHits300.ElementAt(i);
                var pairMisses = resultMisses.ElementAt(i);

                double missesVal = pairMisses.Value * 1.5;
                double gps = (2 * (pairAcc.Value + pairCombo.Value)) + (1.5 * (pairScore.Value + +pairHits300.Value));
                gps -= missesVal;

                if (gps < 0 || double.IsNaN(gps))
                    gps = 0;
                else
                    gps *= 2;

                scores.First(s => s.SeasonPlayerId == pairAcc.Key).GeneralPerformanceScore = gps;
            }
        }

        /// <summary>
        /// Gets team wins
        /// </summary>
        /// <param name="games">games to analyze</param>
        /// <returns><see cref="Tuple{T1, T2}"/> [RedTeamWins, BlueTeamWins]</returns>
        private static Tuple<int, int> GetWins(HistoryJson.Game[] games, int warmupCount)
        {
            int red = 0;
            int blue = 0;
            int warmupCounter = 0;

            foreach (HistoryJson.Game game in games)
            {
                int redScore = 0;
                int blueScore = 0;

                if (warmupCount > 0 && warmupCounter < warmupCount)
                {
                    warmupCounter++;
                    continue;
                }

                foreach (HistoryJson.Score score in game.scores)
                {
                    HistoryJson.Match multiplayer = score.match;

                    if (multiplayer.pass == 0)
                        continue;

                    switch (multiplayer.team)
                    {
                        case "red":
                            redScore += score.score.Value;
                            break;
                        case "blue":
                            blueScore += score.score.Value;
                            break;
                    }
                }

                if (blueScore == redScore)
                    continue;
                else if (blueScore > redScore)
                    blue++;
                else if (redScore > blueScore)
                    red++;
            }
            
            return new Tuple<int, int>(blue, red);
        }
    }
}
