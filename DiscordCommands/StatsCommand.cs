using SkyBot.Discord.CommandSystem;
using SkyBot;
using SkyBot.Database;
using SkyBot.Database.Models;
using SkyBot.Database.Models.Statistics;
using System;
using System.Collections.Generic;
using System.Text;
using DSharpPlus.Entities;
using System.Globalization;
using System.Linq;

namespace DiscordCommands
{
    public class StatsCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => ResourcesCommands.StatsCommand;

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.Public;

        public string Description => ResourcesCommands.StatsCommandDescription;

        public string Usage => ResourcesCommands.StatsCommandUsage;

        public int MinParameters => 2;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            string param = args.Parameters[0].ToLower(CultureInfo.CurrentCulture);
            args.Parameters.RemoveAt(0);

            switch(param)
            {
                default:
                    HelpCommand.ShowHelp(args.Channel, this);
                    return;

                case "player":
                case "p":
                    OnPlayerCommand(args);
                    return;

                case "team":
                case "t":
                    OnTeamCommand(args);
                    return;

                case "match":
                case "m":
                    OnMatchCommand(args);
                    return;
            }
        }

        private void OnPlayerCommand(CommandEventArg args)
        {
            string param = args.Parameters[0].ToLower(CultureInfo.CurrentCulture);
            args.Parameters.RemoveAt(0);

            switch (param)
            {
                case "profile":
                case "p":
                    OnPlayerProfile(args);
                    return;

                case "top":
                case "t":
                    OnPlayerTopList(args);
                    return;

                case "last":
                case "l":
                    OnPlayerTopList(args, true);
                    return;

                default:
                    HelpCommand.ShowHelp(args.Channel, this);
                    return;
            }
        }

        private void OnTeamCommand(CommandEventArg args)
        {
            string param = args.Parameters[0].ToLower(CultureInfo.CurrentCulture);
            args.Parameters.RemoveAt(0);

            switch (param)
            {
                case "profile":
                case "p":
                    OnTeamProfile(args);
                    return;

                case "top":
                case "t":
                    OnTeamTopList(args);
                    return;

                case "last":
                case "l":
                    OnTeamTopList(args, true);
                    return;

                default:
                    HelpCommand.ShowHelp(args.Channel, this);
                    return;
            }
        }

        private static void OnPlayerTopList(CommandEventArg args, bool reverse = false)
        {
            int page = 1;

            if (args.Parameters.Count > 0 && int.TryParse(args.Parameters[0], out int p))
                page = p;

            page--;

            List<SeasonPlayerCardCache> players = GetPlayers(args.Guild).OrderByDescending(sp => sp.OverallRating).ToList();

            if (players.Count == 0)
            {
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                {
                    Title = ResourceStats.NoStatsFound,
                    Description = Resources.InvisibleCharacter
                };

                args.Channel.SendMessageAsync(embed: builder.Build());
                return;
            }
            else if (reverse)
                players.Reverse();

            args.Channel.SendMessageAsync(embed: GetListAsEmbed<SeasonPlayerCardCache>(players, page * 10, 10, ResourceStats.Players,
                                                                                       new Func<SeasonPlayerCardCache, string>(sp => sp.Username),
                                                                                       new Func<SeasonPlayerCardCache, double>(sp => sp.OverallRating)));
        }

        private static void OnTeamTopList(CommandEventArg args, bool reverse = false)
        {
            int page = 1;

            if (args.Parameters.Count > 0 && int.TryParse(args.Parameters[0], out int p))
                page = p;

            page--;

            List<SeasonTeamCardCache> teams = GetTeams(args.Guild).OrderByDescending(sp => sp.TeamRating).ToList();
            if (teams.Count == 0)
            {
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                {
                    Title = ResourceStats.NoStatsFound,
                    Description = Resources.InvisibleCharacter
                };

                args.Channel.SendMessageAsync(embed: builder.Build());
                return;
            }
            if (reverse)
                teams.Reverse();

            args.Channel.SendMessageAsync(embed: GetListAsEmbed<SeasonTeamCardCache>(teams, page * 10, 10, ResourceStats.Teams,
                                                                                       new Func<SeasonTeamCardCache, string>(sp => sp.TeamName),
                                                                                       new Func<SeasonTeamCardCache, double>(sp => Math.Round(sp.TeamRating, 2, MidpointRounding.AwayFromZero))));
        }

        private static void OnPlayerProfile(CommandEventArg args)
        {
            using DBContext c = new DBContext();
            (string, long) userParsed = TryParseIdOrUsernameString(args.Parameters);

            long osuUserId = -1;

            if (userParsed.Item1 != null)
                osuUserId = c.SeasonPlayer.FirstOrDefault(sp => sp.LastOsuUsername.Equals(userParsed.Item1, StringComparison.CurrentCultureIgnoreCase) &&
                                                                sp.DiscordGuildId == (long)args.Guild.Id).OsuUserId;
            else if (userParsed.Item2 != -1)
                osuUserId = userParsed.Item2;

            if (osuUserId == -1)
            {
                args.Channel.SendMessageAsync(ResourceStats.PlayerNotFound + osuUserId);
                return;
            }

            SeasonPlayerCardCache spcc = GetPlayer(args.Guild, osuUserId);

            if (spcc == null)
            {
                args.Channel.SendMessageAsync(ResourceStats.PlayerNotFound + osuUserId);
                return;
            }

            args.Channel.SendMessageAsync(embed: GetPlayerEmbed(spcc.Username, spcc.TeamName, spcc.OsuUserId, spcc.AverageAccuracy, (int)spcc.AverageScore, spcc.AverageMisses, (int)spcc.AverageCombo, spcc.AveragePerformance, spcc.MatchMvps, spcc.OverallRating));
        }

        private static void OnTeamProfile(CommandEventArg args)
        {
            using DBContext c = new DBContext();
            (string, long) userParsed = TryParseIdOrUsernameString(args.Parameters);

            string teamName = userParsed.Item1;

            SeasonTeamCardCache stcc = GetTeam(args.Guild, teamName);

            if (stcc == null)
            {
                args.Channel.SendMessageAsync(ResourceStats.TeamNotFound + teamName);
                return;
            }

            args.Channel.SendMessageAsync(embed: GetTeamEmbed(stcc.TeamName, stcc.AverageAccuracy, (int)stcc.AverageScore, stcc.AverageMisses, (int)stcc.AverageCombo, stcc.AverageGeneralPerformanceScore, stcc.TotalMatchMVPs, stcc.AverageOverallRating, stcc.TeamRating, stcc.MVPName));
        }

        private void OnMatchCommand(CommandEventArg args)
        {
            using DBContext c = new DBContext();
            long matchId = -1;

            //team a vs team b
            if (args.Parameters.Count > 1)
            {
                StringBuilder vsSb = new StringBuilder();

                for (int i = 0; i < args.Parameters.Count; i++)
                    vsSb.Append(" " + args.Parameters[i]);

                vsSb.Remove(0, 1);

                string matchName = vsSb.ToString();

                matchId = c.SeasonResult.FirstOrDefault(sr => sr.MatchName.Equals(matchName, StringComparison.CurrentCultureIgnoreCase) &&
                                                              sr.DiscordGuildId == (long)args.Guild.Id)?.Id ?? -1;
            }
            else //matchid
            {
                if (!int.TryParse(args.Parameters[0], out int mid))
                {
                    HelpCommand.ShowHelp(args.Channel, this);
                    return;
                }

                matchId = mid;

                if (!c.SeasonResult.Any(sr => sr.MatchId == matchId &&
                                              sr.DiscordGuildId == (long)args.Guild.Id))
                {
                    args.Channel.SendMessageAsync(ResourceStats.MatchNotFound + matchId);
                    return;
                }
            }

            DiscordEmbed embed = GetMatchEmbedFromDB((int)matchId);

            if (embed == null)
            {
                DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
                {
                    Title = ResourceStats.MatchNotFound,
                    Description = Resources.InvisibleCharacter
                };

                embed = builder.Build();
            }

            args.Channel.SendMessageAsync(embed: embed);
        }

        private static (string, long) TryParseIdOrUsernameString(List<string> parameters)
        {
            if (long.TryParse(parameters[0], out long id))
                return (null, id);

            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < parameters.Count; i++)
                sb.Append(" " + parameters[i]);

            sb.Remove(0, 1);

            return (sb.ToString(), -1);

        }

        private static List<SeasonPlayerCardCache> GetPlayers(DiscordGuild guild)
        {
            using DBContext c = new DBContext();
            return c.SeasonPlayerCardCache.Where(spcc => spcc.DiscordGuildId == (long)guild.Id).ToList();
        }

        private static List<SeasonTeamCardCache> GetTeams(DiscordGuild guild)
        {
            using DBContext c = new DBContext();
            return c.SeasonTeamCardCache.Where(spcc => spcc.DiscordGuildId == (long)guild.Id).ToList();
        }

        private static SeasonPlayerCardCache GetPlayer(DiscordGuild guild, long osuId)
        {
            using DBContext c = new DBContext();
            return c.SeasonPlayerCardCache.FirstOrDefault(spcc => spcc.DiscordGuildId == (long)guild.Id &&
                                                                  spcc.OsuUserId == osuId);
        }

        private static SeasonTeamCardCache GetTeam(DiscordGuild guild, string teamName)
        {
            using DBContext c = new DBContext();
            return c.SeasonTeamCardCache.FirstOrDefault(stcc => stcc.DiscordGuildId == (long)guild.Id &&
                                                                stcc.TeamName.Equals(teamName, StringComparison.CurrentCultureIgnoreCase));
        }

        private static DiscordEmbed GetMatchEmbedFromDB(int matchId)
        {
            return SkyBot.Analyzer.OsuAnalyzer.GetMatchResultEmbed(matchId);
        }

        private static DiscordEmbed GetPlayerEmbed(string userName, string teamName, long osuUserId, double avgAcc, int avgScore, double avgMisses, int avgCombo, double avgGps, int matchMvps, double overallRating)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Author = new DiscordEmbedBuilder.EmbedAuthor()
                {
                    IconUrl = "",
                    Name = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", ResourceStats.Team, teamName),
                },
                ThumbnailUrl = "https://a.ppy.sh/" + osuUserId,
                Title = string.Format(CultureInfo.CurrentCulture, "{0} {1} ({2})", ResourceStats.StatsFor, userName, osuUserId),
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", ResourceStats.LastUpdated, DateTime.UtcNow)
                }
            };

            avgAcc = Math.Round(avgAcc, 2, MidpointRounding.AwayFromZero);
            avgMisses = Math.Round(avgMisses, 1, MidpointRounding.AwayFromZero);
            avgGps = Math.Round(avgGps, 2, MidpointRounding.AwayFromZero);
            overallRating = Math.Round(overallRating, 2, MidpointRounding.AwayFromZero);

            builder.AddField(ResourceStats.AverageAccuracy, avgAcc.ToString(CultureInfo.CurrentCulture) + " %", true)
                   .AddField(ResourceStats.AverageScore, string.Format(CultureInfo.CurrentCulture, "{0:n0}", avgScore), true)
                   .AddField(ResourceStats.AverageMisses, avgMisses.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.AverageCombo, avgCombo.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.AverageGPS, avgGps.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.MatchMVPs, matchMvps.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.OverallRating, (overallRating.ToString(CultureInfo.CurrentCulture) + $" (+{matchMvps * 3.5})"), true);

            return builder.Build();
        }

        private static DiscordEmbed GetTeamEmbed(string teamName, double avgAcc, int avgScore, double avgMisses, int avgCombo, double avgGps, int totalMvps, double avgRating, double teamRating, string mvpName)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = string.Format(CultureInfo.CurrentCulture, "{0} {1} {2}", ResourceStats.StatsFor, ResourceStats.Team, teamName),
                Description = ResourceStats.TeamMVP + mvpName,
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", ResourceStats.LastUpdated, DateTime.UtcNow)
                }
            };

            avgAcc = Math.Round(avgAcc, 2, MidpointRounding.AwayFromZero);
            avgMisses = Math.Round(avgMisses, 1, MidpointRounding.AwayFromZero);
            avgGps = Math.Round(avgGps, 2, MidpointRounding.AwayFromZero);
            avgRating = Math.Round(avgRating, 2, MidpointRounding.AwayFromZero);
            teamRating = Math.Round(teamRating, 2, MidpointRounding.AwayFromZero);

            builder.AddField(ResourceStats.AverageAccuracy, avgAcc.ToString(CultureInfo.CurrentCulture) + " %", true)
                   .AddField(ResourceStats.AverageScore, string.Format(CultureInfo.CurrentCulture, "{0:n0}", avgScore), true)
                   .AddField(ResourceStats.AverageMisses, avgMisses.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.AverageCombo, avgCombo.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.AverageGPS, avgGps.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.MatchMVPs, totalMvps.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.AverageRating, avgRating.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.TeamRating, (teamRating.ToString(CultureInfo.CurrentCulture) + $" (+{totalMvps * 3.5})"), true);

            return builder.Build();
        }

        private static int GetPage(int start)
        {
            int page = 1;

            if (start >= 10)
            {
                double p = (start + 1) / 10.0;
                page = (int)p;

                if (p > page)
                    page++;
            }

            return page;
        }

        private static int GetMaxPages(int count)
        {
            double mp = count / 10.0;
            int maxPages = (int)mp;

            if (mp > maxPages)
                maxPages++;

            return maxPages;
        }

        private static DiscordEmbed GetListAsEmbed<T>(List<T> input, int start, int count, string listTitle, Func<T, string> nameConverter, Func<T, double> ratingConverter)
        {
            int page = GetPage(start);
            int maxPages = GetMaxPages(input.Count);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"{ResourceStats.Page} {page}/{maxPages}"
                }
            };

            if (start + count >= input.Count)
                count = input.Count - start;
            else if (start >= input.Count)
                return builder.Build();

            builder.Title = $"{ResourceStats.Top} {(page - 1) * 10 + count}/{input.Count} {listTitle}";

            StringBuilder ranksb = new StringBuilder();
            StringBuilder namesb = new StringBuilder();
            StringBuilder ratingsb = new StringBuilder();

            for (int i = start; i < start + count; i++)
            {
                ranksb.AppendLine($"{i + 1}.");
                namesb.AppendLine(nameConverter(input[i]));
                ratingsb.AppendLine(ratingConverter(input[i]).ToString(CultureInfo.CurrentCulture));
            }

            if (ranksb.Length == 0 ||
                namesb.Length == 0 ||
                ratingsb.Length == 0)
            {
                builder.AddField(ResourceStats.StatsUnavailable, ResourceStats.NoStatsFound);
            }
            else
            {
                builder.AddField(ResourceStats.Rank, ranksb.ToString(), true)
                       .AddField(listTitle, namesb.ToString(), true)
                       .AddField(ResourceStats.Rating, ratingsb.ToString(), true);
            }

            return builder.Build();
        }
    }
}
