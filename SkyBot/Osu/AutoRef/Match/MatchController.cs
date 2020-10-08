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
        public bool IsFinished { get; private set; }
        public bool IsLobbyCreated => _isLobbyCreated;
        public DateTime MatchStartTime => _matchStartTime;
        public TimeSpan RunningSince => DateTime.UtcNow.Subtract(_matchCreationTime);
        public int WorkflowIteration { get => _workflowIndex; set => _workflowIndex = value; }
        public int WorkflowMaxIterations { get => _workflow.Count; }


        private LobbyController _controller;
        private MatchSettings _settings;


        private DateTime _matchStartTime;
        private DateTime _matchCreationTime;
        private DateTime _matchInvitationTime;
        private string _matchName;
        private ulong _submissionChannel;

        private bool _isLobbyCreated;

        private List<string> _playersBlue;
        private List<string> _playersRed;

        private string _captainBlue;
        private string _captainRed;
        private string _captainBlueDisplay;
        private string _captainRedDisplay;

        private int _totalPlayers;
        private int _totalWarmups;
        private int _totalRounds;

        private LobbyColor _nextPick;
        private List<long> _bannedMaps;

        private List<LobbyScore> _totalScores;
        private List<LobbyScore> _latestScores;

        private int _blueWins;
        private int _redWins;

        private List<Action> _workflow;
        private int _workflowIndex;
        bool _isTestRun;
        /*
            Warmup Phase: HeadToHead, ScoreV2
            Play Phase: TeamVs, ScoreV2
            Blue team first slots
            Red team second slots
         */

        public MatchController(LobbyController controller, DateTime matchStartTime, string matchName,
                               List<string> playersRed, List<string> playersBlue, string captainRed,
                               string captainBlue, int totalWarmups, int totalRounds, ulong submissionChannel)
        {
            _matchStartTime = matchStartTime;
            _matchCreationTime = matchStartTime.Subtract(TimeSpan.FromMinutes(15));
            _matchInvitationTime = matchStartTime.Subtract(TimeSpan.FromMinutes(5));

            _submissionChannel = submissionChannel;
            _controller = controller;
            _matchStartTime = matchStartTime;
            _matchName = matchName;
            controller.OnLobbyCreated += OnLobbyCreated;
            controller.OnScoreReceived += OnScoreReceived;

            _latestScores = new List<LobbyScore>();
            _totalScores = new List<LobbyScore>();

            if (playersBlue != null)
            {
                _playersBlue = playersBlue;
                FixNames(_playersBlue);
            }
            else
                _playersBlue = new List<string>();

            if (playersRed != null)
            {
                _playersRed = playersRed;
                FixNames(_playersRed);
            }
            else
                _playersRed = new List<string>();

            _captainBlue = FixName(captainBlue);
            _captainRed = FixName(captainRed);

            _captainBlueDisplay = _captainBlue.Replace('_', ' ');
            _captainRedDisplay = _captainRed.Replace('_', ' ');

            _playersBlue.Remove(_captainBlue);
            _playersRed.Remove(_captainRed);

            _totalPlayers = _playersBlue.Count + 1 + _playersRed.Count + 1;
            _totalRounds = totalRounds;
            _totalWarmups = totalWarmups;
            _bannedMaps = new List<long>();

            CreateWorkflow();
        }

        private void AddScore(LobbyScore score)
        {
            _totalScores.Add(score);
            _latestScores.Add(score);
        }

        private void OnScoreReceived(object sender, LobbyScore e)
        {
            AddScore(new LobbyScore(FixName(e.Username), e.Score, e.Passed));
        }

        private void CreateWorkflow()
        {
            _workflow = new List<Action>()
            {
                new Action(WaitForLobbyCreation),
                new Action(InitSetup),
                new Action(WaitForLobbyInvites),
                new Action(InvitePlayers),
                new Action(WaitForPlayersToJoin),
                new Action(SortPlayers),
                new Action(SetTeamColors),
                new Action(WaitForMatchStartTime),
                new Action(() => SendMessage("Welcome to Skybot's auto-ref (Alpha 0.1.1)")),
                new Action(() => SendMessage($"Blue captain: {_captainBlueDisplay} | Red captain: {_captainRedDisplay}")),
                new Action(Setup),
                new Action(GetBans),
                new Action(PlayPhase),
                new Action(SubmitResults),
                new Action(CloseLobby)
            };

            if (_totalWarmups > 0)
            {
                _workflow.Insert(10, new Action(SetupWarmup));
                _workflow.Insert(11, new Action(PlayWarmup));
            }
        }

        public void SetupTestWorkflow()
        {
            _isTestRun = true;
            _matchCreationTime = _matchStartTime.Subtract(TimeSpan.FromMinutes(3));
            _matchInvitationTime = _matchStartTime.Subtract(TimeSpan.FromMinutes(1));
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

        private void SubmitResults()
        {
            Logger.Log("Submitting Results");
            WaitFor(DateTime.UtcNow);
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

            Program.DiscordHandler.GetChannelAsync(_submissionChannel).ConfigureAwait(false).GetAwaiter().GetResult()
                                  .SendMessageAsync(embed: builder.Build());
        }

        private void CloseLobby()
        {
            Logger.Log("Closing lobby");
            SendMessage("Match ended, you have 240 seconds to leave");
            Task.Delay(240 * 1000).ConfigureAwait(false).GetAwaiter().GetResult();

            WaitFor(DateTime.UtcNow);
            _controller.CloseMatch();

            IsFinished = true;
        }

        private void SetupWarmup()
        {
            Logger.Log("Setting up for warmup");
            _controller.SetWinConditions(TeamMode.HeadToHead, WinCondition.ScoreV2, _totalPlayers);
        }

        private void InitSetup()
        {
            Logger.Log("Initial setup");
            WaitFor(DateTime.UtcNow);
            _controller.SetMatchLock(true);
            _controller.SetWinConditions(TeamMode.TeamVs, WinCondition.ScoreV2, 16);
        }

        private void Setup()
        {
            Logger.Log("Setting up for play phase");
            WaitFor(DateTime.UtcNow);
            _controller.SetWinConditions(TeamMode.TeamVs, WinCondition.ScoreV2, _totalPlayers);
            _latestScores.Clear();
            _totalScores.Clear();
            _controller.RefreshSettings();
        }

        private void SortPlayers()
        {
            Logger.Log("Sorting players");
            int playersPerTeam = _totalPlayers / 2;

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
                    WaitFor(DateTime.UtcNow);
                    _controller.SetSlot(slot.Nickname, nextFreeSlot);
                    nextFreeSlot++;
                }
            }

            for (int i = 0; i < bluePlayers.Count; i++)
            {
                LobbySlot nextSlot = blueSlots[0];
                blueSlots.RemoveAt(0);

                WaitFor(DateTime.UtcNow);
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

            _controller.RefreshSettings();
        }

        private void SetTeamColors()
        {
            Logger.Log("Setting team colors");

            SetColors(_playersBlue, LobbyColor.Blue);
            WaitFor(DateTime.UtcNow);
            _controller.SetTeam(_captainBlue, LobbyColor.Blue);

            SetColors(_playersRed, LobbyColor.Red);
            WaitFor(DateTime.UtcNow);
            _controller.SetTeam(_captainRed, LobbyColor.Red);

            WaitFor(DateTime.UtcNow);
            _controller.RefreshSettings();

            void SetColors(List<string> users, LobbyColor color)
            {
                for (int i = 0; i < users.Count; i++)
                {
                    WaitFor(DateTime.UtcNow);
                    _controller.SetTeam(users[i], color);
                }
            }
        }

        private void PlayWarmup()
        {
            Logger.Log("Starting warmup phase");
            SendMessage("Rolls for warmups");
            SetNextPlayerPick();

            for (int i = 0; i < 2; i++)
            {
                long mapId = PickNextMap();
                WaitForPlayersReady(TimeSpan.FromSeconds(120));
                PlayMap(mapId);
                WaitForMapEnd();
            }

            SendMessage("Warmups finished");
            Logger.Log("Warmups finished");
        }

        private void PlayPhase()
        {
            Logger.Log("Play phase");
            WaitFor(DateTime.UtcNow);
            SendMessage("Play phase");
            SendMessage("Rolls for play phase");

            SetNextPlayerPick();

            for (int i = 0; i < _totalRounds; i++)
            {
                WaitFor(DateTime.UtcNow);
                long mapId = PickNextMap();
                WaitFor(DateTime.UtcNow);
                WaitForPlayersReady(TimeSpan.FromSeconds(120));
                WaitFor(DateTime.UtcNow);
                PlayMap(mapId);
                WaitFor(DateTime.UtcNow);
                WaitForMapEnd();
                WaitFor(DateTime.UtcNow);
                GetWins();
            }

            SendMessage("End of play phase");
            Logger.Log("End of play phase");
        }

        private void GetBans()
        {
            Logger.Log("Ban phase");
            SendMessage("Ban phase");

            WaitFor(DateTime.UtcNow);
            SendMessage("Rolls for ban phase");
            SetNextPlayerPick();

            for (int i = 0; i < 2; i++)
            {
                long mapId = PickNextMap();
                _bannedMaps.Add(mapId);

                WaitFor(DateTime.UtcNow);
                SendMessage($"Banned map id {mapId}");
            }

            WaitFor(DateTime.UtcNow);
            SendMessage("End of ban phase");
            Logger.Log("End of ban phase");
        }

        private void PlayMap(long mapId)
        {
            SendCommand($"!mp map {mapId} 0");
            SendMessage("You have 120 seconds to ready up before the game will be force started");

            if (WaitForPlayersReady(TimeSpan.FromSeconds(120)))
                SendMessage("Starting match, good luck!");
            else
                SendMessage("Force Starting match, good luck!");

            SendCommand("!mp start");
        }

        private void WaitForMapEnd()
        {
            SendMessage("Waiting for map end");

            while (_latestScores.Count < _totalPlayers)
                Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
            WaitFor(DateTime.UtcNow);

            SendMessage("Map ended");
        }

        private void GetWins()
        {
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

            WaitFor(DateTime.UtcNow);
            SendMessage($"Current wins: Blue {_blueWins} vs {_redWins} Red");
        }

        /// <param name="timeout">Time until we return with false</param>
        /// <returns>All players ready</returns>
        private bool WaitForPlayersReady(TimeSpan timeout)
        {
            Stopwatch elapsedTime = new Stopwatch();
            elapsedTime.Start();

            int readyCount = 0;
            while((readyCount = _controller.Slots.Count(s => s.Value.IsReady)) < _totalPlayers &&
                  elapsedTime.Elapsed < timeout)
            {
                _controller.RefreshSettings();
                Task.Delay(2500).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            elapsedTime.Stop();

            if (timeout < elapsedTime.Elapsed)
                return false;

            return true;
        }

        private void WaitForMatchStartTime()
        {
            Logger.Log("Waiting for match to start");
            SendMessage("Waiting for match to start");
            WaitFor(_matchStartTime);
        }

        private long PickNextMap()
        {
            long? beatmap = null;
            string nextPick = _nextPick == LobbyColor.Red ? _captainRed : _captainBlue;
            string nextPickDisplay = _nextPick == LobbyColor.Red ? _captainRedDisplay : _captainBlueDisplay;

            WaitFor(DateTime.UtcNow);
            while (!(beatmap = _controller.RequestPick(nextPick, $"{nextPickDisplay} Pick a map via !pick <mapId>")).HasValue)
            {
                WaitFor(DateTime.UtcNow);
                Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            return beatmap.Value;
        }

        private void SendMessage(string message)
        {
            _controller.SendChannelMessage($"——— {message} ———");
        }

        private void SendCommand(string command)
        {
            _controller.SendChannelMessage(command);
        }

        /// <summary>
        /// Sets which captain can pick next
        /// </summary>
        private void SetNextPlayerPick()
        {
            while (!SetPick())
                Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();

            bool SetPick()
            {
                (long, long) rolls = GetCaptainRolls();

                WaitFor(DateTime.UtcNow);

                if (rolls.Item1 > rolls.Item2)
                {
                    _nextPick = LobbyColor.Blue;
                    SendMessage($"{_captainBlueDisplay} won the roll!");
                }
                else if (rolls.Item2 > rolls.Item1)
                {
                    _nextPick = LobbyColor.Red;
                    SendMessage($"{_captainRedDisplay} won the roll!");
                }
                else
                    return false;

                WaitFor(DateTime.UtcNow);
                SendMessage("Invalid roll result");

                return true;
            }
        }

        /// <returns>(Blue, Red)</returns>
        private (long, long) GetCaptainRolls()
        {
            LobbyRoll blueRoll = GetRoll(_captainBlue);
            LobbyRoll redRoll = GetRoll(_captainRed);

            return (blueRoll.Rolled, redRoll.Rolled);
        }

        private LobbyRoll GetRoll(string from)
        {
            WaitFor(DateTime.UtcNow);
            LobbyRoll roll = _controller.RequestRoll(from);

            return roll;
        }

        private void OnLobbyCreated(object sender, EventArgs args)
        {
            _controller.OnLobbyCreated -= OnLobbyCreated;
            _isLobbyCreated = true;
        }

        /// <summary>
        /// Waits until <see cref="_matchCreationTime"/>, creates a lobby and waits for the lobby to be created
        /// </summary>
        private void WaitForLobbyCreation()
        {
            Logger.Log("Waiting for lobby to create");
            WaitFor(_matchCreationTime);

            WaitFor(DateTime.UtcNow);
            Logger.Log("Creating lobby");
            _controller.CreateMatch(_matchName);

            Logger.Log("Waiting for lobby to be created");
            
            while(!_isLobbyCreated)
                Task.Delay(50).ConfigureAwait(false).GetAwaiter().GetResult();

            WaitFor(DateTime.UtcNow);

            Logger.Log("Lobby created");
        }

        private void WaitForLobbyInvites()
        {
            Logger.Log("Waiting until we can invite players");
            WaitFor(_matchInvitationTime);
        }

        /// <summary>
        /// Waits until the specified time
        /// </summary>
        /// <param name="date"></param>
        /// <param name="waitIfNotInLobby">Should we wait until we are reconnected again</param>
        private void WaitFor(DateTime date, bool waitIfNotInLobby = true)
        {
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

        private void InvitePlayers()
        {
            Logger.Log("Inviting players");

            if (_playersBlue.Count > 0)
                InvitePlayers(_playersBlue);

            if (_playersRed.Count > 0)
                InvitePlayers(_playersRed);

            InvitePlayer(_captainBlue);
            InvitePlayer(_captainRed);

            Logger.Log("Invited players");

            void InvitePlayers(List<string> nicknames)
            {
                WaitFor(DateTime.UtcNow);
                for (int i = 0; i < nicknames.Count; i++)
                    InvitePlayer(nicknames[i]);
            }
        }

        private void InvitePlayer(string nickname)
        {
            WaitFor(DateTime.UtcNow);
            _controller.Invite(nickname);
        }

        /// <summary>
        /// Waits until all players have joined
        /// </summary>
        private void WaitForPlayersToJoin()
        {
            Logger.Log("Waiting for players to join");

            while (_controller.Slots.Count(s => s.Value.Nickname != null) < _totalPlayers)
                Task.Delay(100).ConfigureAwait(false).GetAwaiter().GetResult();
            
            WaitFor(DateTime.UtcNow);

            Logger.Log("All players joined");
        }

        private void FixNames(List<string> names)
        {
            for (int i = 0; i < names.Count; i++)
                names[i] = FixName(names[i]);
        }

        private static string FixName(string name)
        {
            return name.Replace(' ', '_');
        }
    }
}
