using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace SkyBot.Osu.AutoRef.Match
{
    public class MatchController
    {
        public bool IsFinished { get; set; }
        public bool IsLobbyCreated => _isLobbyCreated;
        public DateTime MatchStartTime => _settings.MatchStartTime;
        public TimeSpan RunningSince => DateTime.UtcNow.Subtract(_matchCreationTime);
        public int WorkflowIteration { get => _workflowIndex; set => _workflowIndex = value; }
        public int WorkflowMaxIterations { get => _workflow.Count; }


        LobbyController _controller;
        MatchSettings _settings;


        DateTime _matchCreationTime;
        DateTime _matchInvitationTime;

        string _matchName;

        bool _isLobbyCreated;

        List<string> _playersBlue;
        List<string> _playersRed;

        string _captainBlue;
        string _captainRed;
        string _captainBlueDisplay;
        string _captainRedDisplay;

        LobbyColor _nextPick;
        List<long> _bannedMaps;

        List<LobbyScore> _totalScores;
        List<LobbyScore> _latestScores;

        int _blueWins;
        int _redWins;

        List<Action> _workflow;
        int _workflowIndex;
        /*
            Warmup Phase: HeadToHead, ScoreV2
            Play Phase: TeamVs, ScoreV2
            Blue team first slots
            Red team second slots
         */

            //TODO: make lobby settings getting refreshed regularly

        public MatchController(LobbyController controller, MatchSettings settings)
        {
            _settings = settings;

            controller.OnLobbyCreated += OnLobbyCreated;
            controller.OnScoreReceived += OnScoreReceived;

            _controller = controller;

            if (settings.PlayersBlue != null)
            {
                _playersBlue = settings.PlayersBlue.ToList();
                FixNames(_playersBlue);
            }
            else
                _playersBlue = new List<string>();

            if (settings.PlayersRed != null)
            {
                _playersRed = settings.PlayersRed.ToList();
                FixNames(_playersRed);
            }
            else
                _playersRed = new List<string>();

            _captainBlue = FixName(settings.CaptainBlue);
            _captainRed = FixName(settings.CaptainRed);

            _playersBlue.Remove(_captainBlue);
            _playersRed.Remove(_captainRed);

            _captainBlueDisplay = _captainBlue.Replace('_', ' ');
            _captainRedDisplay = _captainRed.Replace('_', ' ');


            _latestScores = new List<LobbyScore>();
            _totalScores = new List<LobbyScore>();
            _bannedMaps = new List<long>();

            _matchCreationTime = settings.MatchStartTime.Subtract(settings.MatchCreationDelay);
            _matchInvitationTime = settings.MatchStartTime.Subtract(settings.MatchInviteDelay);

            if (settings.IsTestRun)
                CreateTestWorkflow();
            else
                CreateWorkflow();
        }

        void AddScore(LobbyScore score)
        {
            _totalScores.Add(score);
            _latestScores.Add(score);
        }

        void OnScoreReceived(object sender, LobbyScore e)
        {
            AddScore(new LobbyScore(FixName(e.Username), e.Score, e.Passed));
        }

        void CreateTestWorkflow()
        {
            _workflow = new List<Action>()
            {
                new Action(InitSetup),

                new Action(DisableSettingsUpdate),
                new Action(WaitForLobbyInvites),

                new Action(EnableSettingsUpdate),
                new Action(InvitePlayers),
                new Action(WaitForPlayersToJoin),
                new Action(SetTeamColors),
                new Action(WaitForMatchStartTime),
                new Action(() => SendMessage("Welcome to Skybot's auto-ref (Alpha 0.1.1)")),
                new Action(() => SendMessage($"Blue captain: {_captainBlueDisplay ?? "null"} | Red captain: {_captainRedDisplay ?? "null"}")),
                new Action(Setup),

                new Action(DisableSettingsUpdate),
                new Action(() => WaitFor(DateTime.UtcNow.AddSeconds(2))),
                new Action(GetBans),
                new Action(TestPickMap)
            };
        }

        void TestPickMap()
        {
            Logger.Log("TestPickMap");
            long mapId = PickNextMap();
            SendMessage($"Map {mapId} was picked");
            SendMessage($"Test run has ended and lobby will close in {_settings.MatchEndDelay.TotalSeconds} seconds");
            CloseLobby();
        }

        void CreateWorkflow()
        {
            _workflow = new List<Action>()
            {
                new Action(InitSetup),

                new Action(DisableSettingsUpdate),
                new Action(WaitForLobbyInvites),

                new Action(EnableSettingsUpdate),
                new Action(InvitePlayers),
                new Action(WaitForPlayersToJoin),
                new Action(SortPlayers),
                new Action(SetTeamColors),
                new Action(WaitForMatchStartTime),
                new Action(() => SendMessage("Welcome to Skybot's auto-ref (Alpha 0.1.1)")),
                new Action(() => SendMessage($"Blue captain: {_captainBlueDisplay} | Red captain: {_captainRedDisplay}")),
                new Action(Setup),

                new Action(DisableSettingsUpdate),
                new Action(() => WaitFor(DateTime.UtcNow.AddSeconds(2))),
                new Action(GetBans),

                new Action(EnableSettingsUpdate),
                new Action(PlayPhase),

                new Action(DisableSettingsUpdate),
                new Action(SubmitResults),
                new Action(CloseLobby)
            };

            if (_settings.TotalWarmups > 0)
            {
                _workflow.Insert(10, new Action(SetupWarmup));
                _workflow.Insert(11, new Action(PlayWarmup));
            }
        }

        public void Run()
        {
            TryRun(out Exception _);
        }

        public bool TryRun(out Exception ex)
        {
            try
            {
                while(_workflowIndex < _workflow.Count)
                {
                    while (!_controller.IRC.IsConnected ||
                           !_controller.IsInLobby)
                        Task.Delay(250).ConfigureAwait(false).GetAwaiter().GetResult();

                    _workflow[_workflowIndex].Invoke();
                    _workflowIndex++;
                }
            }
            catch (Exception e)
            {
                ex = e;
                return false;
            }

            ex = null;
            return true;
        }

        void EnableSettingsUpdate()
        {
            _controller.IsSettingsWatcherPaused = false;
        }


        void DisableSettingsUpdate()
        {
            _controller.IsSettingsWatcherPaused = true;
        }

        void SubmitResults()
        {
            Logger.Log("SubmitResults");
            SendMessage("Submitting Results");

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "Match " + _matchName,
                Description = Resources.InvisibleCharacter
            };

            if (_blueWins > _redWins)
                builder.AddField($"Winning team wins: {_blueWins}", $"Losing team wins: {_redWins}");
            else
                builder.AddField($"Losing team wins: {_redWins}", $"Winning team wins: {_blueWins}");

            builder.AddField($"MP Link: https://osu.ppy.sh/community/matches/{_controller.Settings.MatchId}", Resources.InvisibleCharacter);

            Program.DiscordHandler.GetChannelAsync(_settings.SubmissionChannel).ConfigureAwait(false).GetAwaiter().GetResult()
                                  .SendMessageAsync(embed: builder.Build());
        }

        void CloseLobby()
        {
            Logger.Log("CloseLobby");
            SendMessage($"Match ended, you have {_settings.MatchEndDelay.TotalSeconds} seconds to leave");
            Task.Delay(_settings.MatchEndDelay).ConfigureAwait(false).GetAwaiter().GetResult();

            _controller.CloseMatch();

            IsFinished = true;
        }

        void SetupWarmup()
        {
            Logger.Log("SetupWarmup");
            _controller.SetWinConditions(TeamMode.HeadToHead, WinCondition.ScoreV2, _settings.TotalPlayers);
        }

        void InitSetup()
        {
            Logger.Log("InitSetup");
            _controller.SetMatchLock(true);
            _controller.SetWinConditions(TeamMode.TeamVs, WinCondition.ScoreV2, 16);
        }

        void Setup()
        {
            Logger.Log("Setup");
            _controller.SetWinConditions(TeamMode.TeamVs, WinCondition.ScoreV2, _settings.TotalPlayers);
            _latestScores.Clear();
            _totalScores.Clear();
        }

        void SortPlayers()
        {
            Logger.Log("SortPlayers");
            int playersPerTeam = _settings.TotalPlayers / 2;

            List<LobbySlot> blueSlots = _controller.Slots.Where(s => s.Key > 0 && s.Key <= playersPerTeam).Select(p => p.Value).ToList();
            List<string> bluePlayers = _playersBlue.ToList();
            bluePlayers.Add(_captainBlue);

            int nextFreeSlot = 11;
            for (int i = 0; i < blueSlots.Count; i++)
            {
                LobbySlot slot = blueSlots[i];

                if (string.IsNullOrEmpty(slot.Nickname))
                    continue;

                if (bluePlayers.Contains(slot.Nickname))
                {
                    bluePlayers.Remove(slot.Nickname);
                    blueSlots.RemoveAt(i);
                    i--;
                    continue;
                }
                else
                {
                    _controller.SetSlot(slot.Nickname, nextFreeSlot);
                    nextFreeSlot++;
                }
            }

            for (int i = 0; i < bluePlayers.Count; i++)
            {
                LobbySlot nextSlot = blueSlots[0];
                blueSlots.RemoveAt(0);

                _controller.SetSlot(bluePlayers[i], nextSlot.Slot);
            }

            List<LobbySlot> redSlots = _controller.Slots.Where(s => s.Key > playersPerTeam && s.Key <= playersPerTeam * 2).Select(p => p.Value).ToList();
            List<string> redPlayers = _playersRed.ToList();
            redPlayers.Add(_captainRed);

            for (int i = 0; i < redSlots.Count; i++)
            {
                LobbySlot slot = redSlots[i];

                if (string.IsNullOrEmpty(slot.Nickname) || !_playersRed.Contains(slot.Nickname))
                    continue;

                redPlayers.Remove(slot.Nickname);
                redSlots.RemoveAt(i);
                i--;
            }

            for (int i = 0; i < redSlots.Count; i++)
            {
                string player = redPlayers[0];
                redPlayers.RemoveAt(0);

                _controller.SetSlot(player, redSlots[i].Slot);
            }
        }

        void SetTeamColors()
        {
            Logger.Log("SetTeamColors");
            SetColors(_playersBlue, LobbyColor.Blue);
            _controller.SetTeam(_captainBlue, LobbyColor.Blue);

            SetColors(_playersRed, LobbyColor.Red);
            _controller.SetTeam(_captainRed, LobbyColor.Red);

            void SetColors(List<string> users, LobbyColor color)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    _controller.SetTeam(users[i], color);
                }
            }
        }

        void PlayWarmup()
        {
            Logger.Log("PlayWarmup");
            SendMessage("Rolls for warmups");
            SetNextPlayerPick();

            for (int i = 0; i < _settings.TotalWarmups; i++)
            {
                long mapId = PickNextMap();
                WaitForPlayersReady(_settings.PlayersReadyUpDelay);
                PlayMap(mapId);
                WaitForMapEnd();
            }

            SendMessage("Warmups finished");
        }

        void PlayPhase()
        {
            Logger.Log("PlayPhase");
            SendMessage("Play phase, Rolls for play phase");

            SetNextPlayerPick();

            for (int i = 0; i < _settings.TotalRounds; i++)
            {
                long mapId = PickNextMap();
                WaitForPlayersReady(_settings.PlayersReadyUpDelay);
                PlayMap(mapId);
                WaitForMapEnd();
                GetWins();
            }

            SendMessage("End of play phase");
        }

        void GetBans()
        {
            Logger.Log("GetBans");
            SendMessage("Ban phase, Rolls for ban phase");
            SetNextPlayerPick();

            for (int i = 0; i < 2; i++)
            {
                long mapId = PickNextMap();
                _bannedMaps.Add(mapId);

                SendMessage($"Removed map id {mapId}");
            }

            SendMessage("End of ban phase");
        }

        void PlayMap(long mapId)
        {
            Logger.Log("PlayMap");
            SendCommand($"!mp map {mapId} 0");
            SendMessage("You have 120 seconds to ready up before the game will be force started");

            if (WaitForPlayersReady(TimeSpan.FromSeconds(120)))
                SendMessage("Starting match, good luck!");
            else
                SendMessage("Force Starting match, good luck!");

            SendCommand("!mp start");
        }

        void WaitForMapEnd()
        {
            Logger.Log("WaitForMapEnd");
            SendMessage("Waiting for map end");

            while (_latestScores.Count < _settings.TotalPlayers)
                Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();

            SendMessage("Map ended");
        }

        void GetWins()
        {
            Logger.Log("GetWins");
            SendMessage("Getting wins...");

            long blueScore = 0;
            long redScore = 0;

            for (int i = 0; i < _latestScores.Count; i++)
            {
                if (!_latestScores[i].Passed)
                    continue;

                string username = _latestScores[i].Username;
                long score = _latestScores[i].Score;

                if (_playersBlue.Contains(username))
                    blueScore += score;
                else if (_playersRed.Contains(username))
                    redScore += score;
            }

            _latestScores.Clear();

            if (blueScore > redScore)
                _blueWins++;
            else if (redScore > blueScore)
                _redWins++;

            SendMessage($"Current wins: Blue {_blueWins} vs {_redWins} Red");
        }

        /// <param name="timeout">Time until we return with false</param>
        /// <returns>All players ready</returns>
        bool WaitForPlayersReady(TimeSpan timeout)
        {
            Logger.Log("WaitForPlayersReady");
            Stopwatch elapsedTime = new Stopwatch();
            elapsedTime.Start();

            int readyCount = 0;
            while((readyCount = _controller.Slots.Count(s => s.Value.IsReady)) < _settings.TotalPlayers &&
                  elapsedTime.Elapsed < timeout)
            {
                Task.Delay(250).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            elapsedTime.Stop();

            if (timeout < elapsedTime.Elapsed)
                return false;

            return true;
        }

        void WaitForMatchStartTime()
        {
            Logger.Log("WaitForMatchStartTime");
            SendMessage("Waiting for match to start");
        }

        long PickNextMap()
        {
            Logger.Log("PickNextMap");
            long? beatmap = null;
            string nextPick = _nextPick == LobbyColor.Red ? _captainRed : _captainBlue;
            string nextPickDisplay = _nextPick == LobbyColor.Red ? _captainRedDisplay : _captainBlueDisplay;

            _nextPick = _nextPick == LobbyColor.Red ? LobbyColor.Blue : LobbyColor.Red;

            while (!(beatmap = _controller.RequestPick(nextPick, $"{nextPickDisplay} Pick a map via !pick <mapId>")).HasValue)
            {
                Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return beatmap.Value;
        }

        void SendMessage(string message)
        {
            _controller.SendChannelMessage($"——— {message} ———");
        }

        void SendCommand(string command)
        {
            _controller.SendChannelMessage(command);
        }

        /// <summary>
        /// Sets which captain can pick next
        /// </summary>
        void SetNextPlayerPick()
        {
            Logger.Log("SetNextPlayerPick");
            while (!SetPick())
                Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();

            bool SetPick()
            {
                (long, long) rolls = GetCaptainRolls();

                if (rolls.Item1 > rolls.Item2)
                {
                    _nextPick = LobbyColor.Blue;
                    SendMessage($"{_captainBlueDisplay} won the roll!");
                    return true;
                }
                else if (rolls.Item2 > rolls.Item1)
                {
                    _nextPick = LobbyColor.Red;
                    SendMessage($"{_captainRedDisplay} won the roll!");
                    return true;
                }
                else
                {
                    SendMessage("Invalid roll result");
                    return false;
                }
            }
        }

        /// <returns>(Blue, Red)</returns>
        (long, long) GetCaptainRolls()
        {
            Logger.Log("GetCaptainRoll");
            LobbyRoll blueRoll = GetRoll(_captainBlue);
            LobbyRoll redRoll = GetRoll(_captainRed);

            return (blueRoll.Rolled, redRoll.Rolled);
        }

        LobbyRoll GetRoll(string from)
        {
            Logger.Log("GetRoll");
            LobbyRoll roll = _controller.RequestRoll(from);

            return roll;
        }

        void OnLobbyCreated(object sender, EventArgs args)
        {
            Logger.Log("OnLobbyCreated");
            _controller.OnLobbyCreated -= OnLobbyCreated;
            _isLobbyCreated = true;
        }

        /// <summary>
        /// Waits until <see cref="_matchCreationTime"/>, creates a lobby and waits for the lobby to be created
        /// </summary>
        void WaitForLobbyCreation()
        {
            Logger.Log("WaitForLobbyCreation");
            WaitFor(_matchCreationTime);
            _controller.CreateMatch(_matchName);

            while(!_isLobbyCreated)
                Task.Delay(50).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        void WaitForLobbyInvites()
        {
            Logger.Log("WaitForLobbyInvites");
            WaitFor(_matchInvitationTime);
        }

        /// <summary>
        /// Waits until the specified time
        /// </summary>
        /// <param name="date">if left empty will use <see cref="DateTime.UtcNow"/></param>
        /// <param name="waitIfNotInLobby">Should we wait until we are reconnected again</param>
        void WaitFor(DateTime date = default, bool waitIfNotInLobby = true)
        {
            if (date == default)
                date = DateTime.UtcNow;

            if (DateTime.UtcNow < date)
            {
                TimeSpan waitTime = date.Subtract(DateTime.UtcNow);
                Task.Delay(waitTime).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            if (waitIfNotInLobby)
            {
                while (!_controller.IRC.IsConnected ||
                       !_controller.IsInLobby)
                    Task.Delay(50).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        void InvitePlayers()
        {
            if (_playersBlue.Count > 0)
                InvitePlayers(_playersBlue);

            if (_playersRed.Count > 0)
                InvitePlayers(_playersRed);

            InvitePlayer(_captainBlue);
            InvitePlayer(_captainRed);

            void InvitePlayers(List<string> nicknames)
            {
                for (int i = 0; i < nicknames.Count; i++)
                    InvitePlayer(nicknames[i]);
            }
        }

        void InvitePlayer(string nickname)
        {
            Logger.Log("InvitePlayer " + nickname);
            _controller.Invite(nickname);
        }

        /// <summary>
        /// Waits until all players have joined
        /// </summary>
        void WaitForPlayersToJoin()
        {
            while (_controller.Slots.Count(s => s.Value.Nickname != null) < _settings.TotalPlayers)
            {
                Task.Delay(250).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        void FixNames(List<string> names)
        {
            for (int i = 0; i < names.Count; i++)
                names[i] = FixName(names[i]);
        }

        static string FixName(string name)
        {
            return name.Replace(' ', '_');
        }
    }
}
