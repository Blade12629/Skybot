using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SkyBot.Database.Models.Statistics;

namespace Skybot.Web.Pages.Api.Session.Data
{
    public struct SessionResult
    {
        public long DiscordGuildId { get; }
        public long MatchId { get; }
        public string MatchName { get; }
        public string Stage { get; }
        public string WinningTeam { get; }
        public string LosingTeam { get; }
        public DateTime TimeStamp { get; }

        public SessionBeatmap[] Beatmaps { get; set; }
        public SessionTeam[] Teams { get; set; }

        public SessionResult(long discordGuildId, long matchId, string matchName, string stage, string winningTeam, string losingTeam, DateTime timeStamp) : this()
        {
            DiscordGuildId = discordGuildId;
            MatchId = matchId;
            MatchName = matchName;
            Stage = stage;
            WinningTeam = winningTeam;
            LosingTeam = losingTeam;
            TimeStamp = timeStamp;
        }

        public static SessionResult FromResult(SeasonResult r)
        {
            if (r == null)
                throw new ArgumentNullException(nameof(r));

            using DBContext c = new DBContext();

            SessionResult result = new SessionResult(r.DiscordGuildId, r.MatchId, r.MatchName, r.Stage, r.WinningTeam, r.LosingTeam, r.TimeStamp);
            List<SeasonScore> scores = c.SeasonScore.Where(s => s.SeasonResultId == r.Id).ToList();

            //Convert players + maps and add maps to result
            List<long> scoreUserIds = scores.Select(s => s.SeasonPlayerId).Distinct().ToList();
            List<long> scoreMapIds = scores.Select(s => s.BeatmapId).Distinct().ToList();

            List<SeasonPlayer> players = new List<SeasonPlayer>(scoreUserIds.Count);
            List<SeasonBeatmap> maps = new List<SeasonBeatmap>(scoreMapIds.Count);

            for (int i = 0; i < scoreUserIds.Count; i++)
                players.Add(c.SeasonPlayer.First(p => p.Id == scoreUserIds[i]));

            for (int i = 0; i < scoreMapIds.Count; i++)
                maps.Add(c.SeasonBeatmap.FirstOrDefault(b => b.Id == scoreMapIds[i]));

            result.Beatmaps = maps.Select(m => (SessionBeatmap)m).ToArray();

            //Convert teams and add them to the result
            Dictionary<string, List<SeasonPlayer>> teams = players.GroupBy(p => p.TeamName).ToDictionary(p => p.Key, p => p.ToList());

            string teamA = teams.Keys.ElementAt(0);
            string teamB = teams.Keys.ElementAt(1);

            result.Teams = new SessionTeam[]
            {
                new SessionTeam(teamA, teams[teamA].Select(p => (SessionPlayer)p).ToArray()),
                new SessionTeam(teamB, teams[teamB].Select(p => (SessionPlayer)p).ToArray()),
            };
            //

            //Convert scores and add them + players to the teams
            Dictionary<long, List<SeasonScore>> uscores = scores.GroupBy(s => s.SeasonPlayerId).ToDictionary(s => s.Key, s => s.ToList());
            Dictionary<long, List<SessionScore>> rscores = new Dictionary<long, List<SessionScore>>(uscores.Count);

            for (int i = 0; i < uscores.Keys.Count; i++)
            {
                long key = uscores.Keys.ElementAt(i);
                List<SeasonScore> scs = uscores[key];

                key = players.First(p => p.Id == key).OsuUserId;

                rscores.Add(key, scs.Select(s => (SessionScore)s).ToList());
            }

            for (int i = 0; i < result.Teams.Length; i++)
                for (int x = 0; x < result.Teams[x].Players.Length; x++)
                    result.Teams[x].Players[i].Scores = rscores[result.Teams[x].Players[i].OsuUserId].ToArray();
            //

            return result;
        }
    }

    public struct SessionBeatmap
    {
        public long BeatmapId { get; }
        public string Author { get; }
        public string Title { get; }
        public string Difficulty { get; }
        public double DifficultyRating { get; }

        public SessionBeatmap(long beatmapId, string author, string title, string difficulty, double difficultyRating) : this()
        {
            BeatmapId = beatmapId;
            Author = author;
            Title = title;
            Difficulty = difficulty;
            DifficultyRating = difficultyRating;
        }

        public static explicit operator SessionBeatmap(SeasonBeatmap m)
        {
            return new SessionBeatmap(m.BeatmapId, m.Author, m.Title, m.Difficulty, m.DifficultyRating);
        }
    }

    public struct SessionTeam
    {
        public string Name { get; }
        public SessionPlayer[] Players { get; set; }

        public SessionTeam(string name, SessionPlayer[] players) : this()
        {
            Name = name;
            Players = players;
        }
    }

    public struct SessionPlayer
    {
        public long OsuUserId { get; }
        public string OsuUsername { get; }
        public string Team { get; }

        public SessionScore[] Scores { get; set; }

        public SessionPlayer(long osuUserId, string osuUsername, string team, SessionScore[] scores) : this()
        {
            OsuUserId = osuUserId;
            OsuUsername = osuUsername;
            Team = team;
            Scores = scores;
        }

        public static explicit operator SessionPlayer(SeasonPlayer p)
        {
            return new SessionPlayer(p.OsuUserId, p.LastOsuUsername, p.TeamName, null);
        }
    }

    public struct SessionScore
    {

        public long BeatmapId { get; set; }

        public bool TeamVs { get; set; }
        public int PlayOrder { get; set; }
        public double GeneralPerformanceScore { get; set; }
        public bool HighestGeneralPerformanceScore { get; set; }
        public float Accuracy { get; set; }
        public long Score { get; set; }
        public int MaxCombo { get; set; }
        public int Perfect { get; set; }
        public DateTime PlayedAt { get; set; }
        public int Pass { get; set; }
        public int Count50 { get; set; }
        public int Count100 { get; set; }
        public int Count300 { get; set; }
        public int CountGeki { get; set; }
        public int CountKatu { get; set; }
        public int CountMiss { get; set; }

        public SessionScore(long beatmapId, bool teamVs, int playOrder, double generalPerformanceScore, bool highestGeneralPerformanceScore, float accuracy, long score, int maxCombo, int perfect, DateTime playedAt, int pass, int count50, int count100, int count300, int countGeki, int countKatu, int countMiss) : this()
        {
            BeatmapId = beatmapId;
            TeamVs = teamVs;
            PlayOrder = playOrder;
            GeneralPerformanceScore = generalPerformanceScore;
            HighestGeneralPerformanceScore = highestGeneralPerformanceScore;
            Accuracy = accuracy;
            Score = score;
            MaxCombo = maxCombo;
            Perfect = perfect;
            PlayedAt = playedAt;
            Pass = pass;
            Count50 = count50;
            Count100 = count100;
            Count300 = count300;
            CountGeki = countGeki;
            CountKatu = countKatu;
            CountMiss = countMiss;
        }
    
        public static explicit operator SessionScore(SeasonScore s)
        {
            return new SessionScore(s.BeatmapId, s.TeamVs, s.PlayOrder, s.GeneralPerformanceScore, s.HighestGeneralPerformanceScore, s.Accuracy, s.Score, s.MaxCombo, s.Perfect, s.PlayedAt, s.Pass, s.Count50, s.Count100, s.Count300, s.CountGeki, s.CountKatu, s.CountMiss);
        }
    }
}
