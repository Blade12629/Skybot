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

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            //TODO: top list for player/teams
            //TODO: card for player/teams/match result
        }

        private List<SeasonPlayerCardCache> GetPlayers(DiscordGuild guild)
        {
            using DBContext c = new DBContext();
            return c.SeasonPlayerCardCache.Where(spcc => spcc.DiscordGuildId == (long)guild.Id).ToList();
        }

        private List<SeasonTeamCardCache> GetTeams(DiscordGuild guild)
        {
            using DBContext c = new DBContext();
            return c.SeasonTeamCardCache.Where(spcc => spcc.DiscordGuildId == (long)guild.Id).ToList();
        }

        private SeasonPlayerCardCache GetPlayer(DiscordGuild guild, long osuId)
        {
            using DBContext c = new DBContext();
            return c.SeasonPlayerCardCache.FirstOrDefault(spcc => spcc.DiscordGuildId == (long)guild.Id &&
                                                                  spcc.OsuUserId == osuId);
        }

        private SeasonPlayerCardCache GetPlayer(DiscordGuild guild, string userName)
        {
            using DBContext c = new DBContext();
            return c.SeasonPlayerCardCache.FirstOrDefault(spcc => spcc.DiscordGuildId == (long)guild.Id &&
                                                                  spcc.Username.Equals(userName, StringComparison.CurrentCultureIgnoreCase));
        }

        private SeasonTeamCardCache GetTeam(DiscordGuild guild, string teamName)
        {
            using DBContext c = new DBContext();
            return c.SeasonTeamCardCache.FirstOrDefault(stcc => stcc.DiscordGuildId == (long)guild.Id &&
                                                                stcc.TeamName.Equals(teamName, StringComparison.CurrentCultureIgnoreCase));
        }

        private DiscordEmbed GetPlayerEmbed(string userName, string teamName, long osuUserId, double avgAcc, int avgScore, double avgMisses, int avgCombo, double avgGps, int matchMvps, double overallRating)
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
                   .AddField(ResourceStats.OverallRating, (overallRating.ToString(CultureInfo.CurrentCulture) + $"({matchMvps * 3.5})"), true);

            return builder.Build();
        }

        private DiscordEmbed GetTeamEmbed(string teamName, double avgAcc, int avgScore, double avgMisses, int avgCombo, double avgGps, int avgMvps, double avgRating)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = string.Format(CultureInfo.CurrentCulture, "{0} {1} {2}", ResourceStats.StatsFor, ResourceStats.Team, teamName),
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = string.Format(CultureInfo.CurrentCulture, "{0}: {1}", ResourceStats.LastUpdated, DateTime.UtcNow)
                }
            };

            avgAcc = Math.Round(avgAcc, 2, MidpointRounding.AwayFromZero);
            avgMisses = Math.Round(avgMisses, 1, MidpointRounding.AwayFromZero);
            avgGps = Math.Round(avgGps, 2, MidpointRounding.AwayFromZero);
            avgRating = Math.Round(avgRating, 2, MidpointRounding.AwayFromZero);

            builder.AddField(ResourceStats.AverageAccuracy, avgAcc.ToString(CultureInfo.CurrentCulture) + " %", true)
                   .AddField(ResourceStats.AverageScore, string.Format(CultureInfo.CurrentCulture, "{0:n0}", avgScore), true)
                   .AddField(ResourceStats.AverageMisses, avgMisses.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.AverageCombo, avgCombo.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.AverageGPS, avgGps.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.MatchMVPs, avgMvps.ToString(CultureInfo.CurrentCulture), true)
                   .AddField(ResourceStats.OverallRating, (avgRating.ToString(CultureInfo.CurrentCulture) + $"({avgMvps * 3.5})"), true);

            return builder.Build();
        }

        private int GetPage(int start)
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

        private int GetMaxPages(int count)
        {
            double mp = count / 10.0;
            int maxPages = (int)mp;

            if (mp > maxPages)
                maxPages++;

            return maxPages;
        }

        private DiscordEmbed GetList<T>(List<T> input, int start, int count, string listTitle, Func<T, string> nameConverter, Func<T, double> ratingConverter)
        {
            int page = GetPage(start);
            int maxPages = GetMaxPages(input.Count);

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = $"{ResourceStats.Top} {page * 10}/{input.Count} {listTitle}s",
                Footer = new DiscordEmbedBuilder.EmbedFooter()
                {
                    Text = $"{ResourceStats.Page} {page}/{maxPages}"
                }
            };

            if (start + count >= input.Count)
                count = input.Count - start;
            else if (start >= input.Count)
                return builder.Build();

            StringBuilder ranksb = new StringBuilder();
            StringBuilder namesb = new StringBuilder();
            StringBuilder ratingsb = new StringBuilder();

            for (int i = start; i < start + count; i++)
            {
                ranksb.AppendLine($"{i + 1}.");
                namesb.AppendLine(nameConverter(input[i]));
                ratingsb.Append(ratingConverter(input[i]));
            }

            builder.AddField(ResourceStats.Rank, ranksb.ToString(), true)
                   .AddField(listTitle, namesb.ToString(), true)
                   .AddField(ResourceStats.Rating, ratingsb.ToString(), true);

            return builder.Build();
        }
    }
}
