using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OsuHistoryEndPoint.Data;

namespace OsuHistoryEndPoint
{
    /// <summary>
    /// Used to get all data including the HistoryJson
    /// </summary>
    public static class GetData
    {
        /// <summary>
        /// Gets the User with the userid <paramref name="userID"/>
        /// </summary>
        public static HistoryUser GetUser(int userID, History history)
        {
            return history.Users.FirstOrDefault(hu => hu.UserId == userID);
        }
        
        /// <summary>
        /// Gets the User from <paramref name="score"/>
        /// </summary>
        public static HistoryUser GetUser(HistoryScore score, History history)
        {
            return GetUser(score.UserId, history);
        }

        /// <summary>
        /// Gets all Matches from <paramref name="history"/>
        /// </summary>
        /// <returns>Array</returns>
        public static List<HistoryGame> GetMatches(History history)
        {
            return history.Events.Where(he => he.Detail.Type.Equals("other", StringComparison.CurrentCultureIgnoreCase))
                                 .Select(he => he.Game)
                                 .ToList();
        }

        /// <summary>
        /// Gets the Match Names
        /// </summary>
        public static List<string> GetMatchNames(History history)
        {
            return history.Events.Where(he => he.Detail.Type.Equals("other", StringComparison.CurrentCultureIgnoreCase))
                                 .Select(he => he.Detail.MatchName)
                                 .ToList();
        }

        /// <summary>
        /// Gets the Event when the Match was created
        /// </summary>
        public static HistoryEvent GetMatchCreatedEvent(History history)
            => history.Events[0];

        /// <summary>
        /// Gets the Event when the Match was disbanded (Last event)
        /// </summary>
        public static HistoryEvent GetMatchDisbandedEvents(History history)
            => history.Events[history.Events.Count() - 1];

        /// <summary>
        /// Gets the wins for both teams
        /// </summary>
        /// <returns>KeyValuePair TeamBlue TeamRed</returns>
        public static KeyValuePair<int, int> GetWins(History history)
        {
            int teamRedWins = 0;
            int teamBlueWins = 0;

            foreach (HistoryEvent curEvent in history.Events)
            {
                if (curEvent.Game == null)
                    continue;

                HistoryGame curGame = curEvent.Game;

                if (curGame.Scores == null)
                    continue;

                int teamRedCurrent = 0;
                int teamBlueCurrent = 0;

                foreach(HistoryScore CurScore in curGame.Scores)
                {
                    switch(CurScore.Match.Team.ToLower())
                    {
                        case "red":
                            teamRedCurrent++;
                            break;

                        case "blue":
                            teamBlueCurrent++;
                            break;
                    }
                }

                if (teamRedCurrent > teamBlueCurrent)
                    teamRedWins++;
                else if (teamBlueCurrent > teamRedCurrent)
                    teamBlueWins++;
            }
            return new KeyValuePair<int, int>(teamBlueWins, teamRedWins);
        }

        /// <summary>
        /// Gets both team names for tournaments (matchname format needs to be: "ASDF: (Team1) vs (Team2)"
        /// </summary>
        /// <param name="history"></param>
        /// <returns>team1, team2</returns>
        public static KeyValuePair<string, string> GetTeamNames(History history)
        {
            string[] matchName = history.Events.First(CurEvent => CurEvent.Detail.Type == "other").Detail.MatchName.Split(' ');

            int vsIndex = 0;

            string teamRed = "";
            string teamBlue = "";

            for (int i = 1; i < matchName.Count(); i++)
            {
                string matchPart = matchName[i];

                if (matchPart.ToLower().Equals("vs"))
                {
                    vsIndex = i;
                    continue;
                }

                if (vsIndex == 0)
                    teamRed += string.Format(" {0}", matchPart);
                else
                    teamBlue += string.Format(" {0}", matchPart);
            }
            teamRed = teamRed.TrimStart('(').TrimEnd(')');
            teamBlue = teamBlue.TrimStart('(').TrimEnd(')');

            return new KeyValuePair<string, string>(teamRed, teamBlue);
        }

        /// <summary>
        /// Gets all played beatmaps
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        public static List<HistoryBeatmap> GetAllBeatMaps(History history)
        {
            return history.Events.Where(ev => ev.Game != null && ev.Game.Beatmap != null)
                                 .Select(ev => ev.Game.Beatmap)
                                 .ToList();
        }

        /// <summary>
        /// Gets the amount of players
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        public static int PlayerCount(History history)
            => history.Users.Length;

        /// <summary>
        /// Gets the amount of games played
        /// </summary>
        /// <param name="history"></param>
        /// <returns></returns>
        public static int GamesPlayed(History history)
        {
            return history.Events.Count(he => he.Game != null && he.Game.Scores != null && he.Game.Scores.Length > 0);
        }

        /// <summary>
        /// Gets the count how many times Players left the game
        /// </summary>
        public static int CountPlayerLeft(History history)
        {
            return history.Events.Count(he => he.Detail.Type.Equals("player-left", StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Gets the count how many time Players joined the game
        /// </summary>
        public static int CountPlayerJoined(History history)
        {
            return history.Events.Count(he => he.Detail.Type.Equals("player-joined", StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Parses the Json from a Url ex.: 000000/history
        /// </summary>
        public static History FromUrl(string Url, System.Net.WebClient wc)
        {
            bool dispose = false;
            if (wc == null)
            {
                wc = new System.Net.WebClient();
                wc.Headers[System.Net.HttpRequestHeader.Accept] = "application/json";
                dispose = true;
            }

            string[] urlSplit = Url.Split('/');

            History history;
            string json;
            try
            {
                if (!long.TryParse(urlSplit[urlSplit.Length - 1], out long mId))
                    throw new ArgumentException("Could not parse match id from url", nameof(Url));

                Url = Url.TrimEnd('/') + "/history";
                json = wc.DownloadString(Url);
                history = JsonConvert.DeserializeObject<History>(json);

                List<History> histories = new List<History>(2);

                HistoryEvent firstEvent = history.Events[0];
                while(firstEvent.Detail == null || !firstEvent.Detail.Type.Equals("match-created", StringComparison.CurrentCultureIgnoreCase))
                {
                    json = wc.DownloadString($"{Url}?before={firstEvent.EventId}");
                    histories.Add(JsonConvert.DeserializeObject<History>(json));
                    firstEvent = histories[histories.Count - 1].Events[0];
                }

                if (histories.Count > 0)
                {
                    List<HistoryEvent> events = new List<HistoryEvent>(history.Events);
                    List<HistoryUser> users = new List<HistoryUser>(history.Users);

                    for (int i = 0; i < histories.Count; i++)
                    {
                        events.AddRange(histories[i].Events);
                        users.AddRange(histories[i].Users);
                    }

                    history.Events = events.ToArray();
                    history.Users = users.ToArray();
                }

                for (int i = 0; i < history.Events.Length; i++)
                {
                    if (history.Events[i].Game == null)
                        continue;

                    for (int x = 0; x < history.Events[i].Game.Scores.Length; x++)
                        history.Events[i].Game.Scores[x].Accuracy *= 100f;
                }

                history.CurrentGameId = mId;
            }
            finally
            {
                if (dispose)
                    wc.Dispose();
            }

            return history;
        }
    }
}
