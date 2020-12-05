using SkyBot.Networking.Irc;
using SkyBot.Osu.IRC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Diagnostics;
using IRCClient = SkyBot.Osu.IRC.OsuIrcClient;
using SkyBot.Osu.AutoRef.Chat;
using AutoRefTypes;
using SkyBot.Osu.AutoRef.Data;

namespace SkyBot.Osu.AutoRef
{
    public partial class LobbyController : ILobby
    {
        public event EventHandler OnBeforeCreating;
        public event EventHandler OnBeforeClosing;
        public event EventHandler OnBeforeTick;
        public event EventHandler OnAfterTick;
        public event EventHandler<Exception> OnException;
        public event EventHandler OnAllPlayersReady;

        public event EventHandler OnLobbyCreated;
        public event EventHandler<ChatMessage> OnMessageReceived;

        public DateTime CreationDate { get; private set; }
        public bool IsLobbyCreated { get; private set; }
        public bool IsLobbyClosed { get; private set; }
        public ILobbySettings Settings { get => _settings; }
        public IReadOnlyList<ISlot> UsedSlots { get => _slots.Values.Where(s => s.IsUsed).Select(s => (ISlot)s).ToList(); }
        public IReadOnlyList<ISlot> Slots => _slots.Values.Select(s => (ISlot)s).ToList();
        public IReadOnlyList<IScore> Scores => _totalScores.Select(s => (IScore)s).ToList();
        public List<Score> LatestScores => _latestScores;
        public List<ChatMessageAction> ChatMessageActions { get; private set; }
        public bool MapFinished { get; private set; }
        public int BlueWins { get; private set; }
        public int RedWins { get; private set; }


        public bool IsMapFinished => throw new NotImplementedException();

        Dictionary<int, Slot> _slots;
        List<Score> _totalScores;
        List<Score> _latestScores;
        int _playersAtMapStart;

        IRCClient _irc;

        Task _tickTask;
        CancellationTokenSource _tickSource;
        bool _shouldTick;
        int _tickDelay;
        ConcurrentQueue<Action> _tickQueue;
        Settings _settings;

        public LobbyController(IRCClient irc, int tickDelay = 100)
        {
            ChatMessageActions = ChatActions.ToList(this);
            _settings = new Settings();
            IsLobbyClosed = true;
            _slots = new Dictionary<int, Slot>();
            _irc = irc;
            _tickDelay = tickDelay;
            _totalScores = new List<Score>();
            _latestScores = new List<Score>();

            for (int i = 0; i < 16; i++)
                _slots.Add(i + 1, new Slot(i + 1));
        }

        public void CreateLobby(string roomName)
        {
            if (_shouldTick)
                return;

            _shouldTick = true;
            OnBeforeCreating?.Invoke(this, null);

            _settings.Reset();
            _settings.RoomName = roomName;
            _tickSource = new CancellationTokenSource();
            _tickTask = new Task(Tick, _tickSource.Token);
            _tickQueue = new ConcurrentQueue<Action>();

            CreateMatch();

            _tickTask.Start();
        }

        public void EnqueueCloseLobby()
        {
            if (!_shouldTick)
                return;

            OnBeforeClosing?.Invoke(this, null);
            Close();

            _tickQueue.Enqueue(() =>
            {
                _shouldTick = false;
                _tickSource.Cancel();
                _irc.OnChannelMessageReceived -= MessageReceived;
            });
        }

        /// <summary>
        /// Converts a nickname to an irc nickname
        /// </summary>
        /// <returns>Example: User 1 -> User_1</returns>
        public string ToIrcNick(string user)
        {
            if (string.IsNullOrEmpty(user))
                return null;

            return user.Replace(' ', '_');
        }

        /// <summary>
        /// Converts an irc nickname to a nickname
        /// </summary>
        /// <returns>Example: User_1 -> User 1</returns>
        public string FromIrcNick(string user)
        {
            if (string.IsNullOrEmpty(user))
                return null;

            return user.Replace('_', ' ');
        }

        /// <summary>
        /// Sends a message at the next tick
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(string message)
        {
            if (!IsLobbyCreated)
                throw new Exception("MP Lobby doesn't exist, cannot send mp command");

            _tickQueue.Enqueue(new Action(() => _irc.SendMessageAsync(Settings.ChannelName, message).ConfigureAwait(false)));
        }

        public Slot GetSlot(string user)
        {
            return _slots.Values.FirstOrDefault(s => s.Nickname != null && s.Nickname.Equals(user, StringComparison.CurrentCultureIgnoreCase));
        }

        public void UserMoved(string user, int slot)
        {
            Slot s = GetSlot(user);
            Slot sn = _slots[slot];

            s.Move(sn);
        }

        public void UserJoined(string user, int slot, SlotColor team)
        {
            Slot s = _slots[slot];
            s.Nickname = user;
            s.Color = team;
        }

        public void UserLeft(string user)
        {
            Slot s = GetSlot(user);
            s.Reset();
        }

        public void MapStarted()
        {
            MapFinished = false;

            if (_latestScores.Count > 0)
            {
                _totalScores.AddRange(_latestScores);
                _latestScores.Clear();
            }
        }

        public void FinishedMap()
        {
            long bluePoints = 0, redPoints = 0;

            for (int i = 0; i < _latestScores.Count; i++)
            {
                if (!_latestScores[i].Passed)
                    continue;

                Slot s = GetSlot(_latestScores[i].Username);

                if (s == null)
                {
                    Logger.Log("Unable to find slot for player", LogLevel.Warning);
                    continue;
                }

                switch (s.Color)
                {
                    case SlotColor.Blue:
                        bluePoints += _latestScores[i].UserScore;
                        break;

                    case SlotColor.Red:
                        redPoints += _latestScores[i].UserScore;
                        break;
                }
            }

            _totalScores.AddRange(_latestScores);
            _latestScores.Clear();

            if (bluePoints > redPoints)
                BlueWins++;
            else if (redPoints > bluePoints)
                RedWins++;
            else
            {
                BlueWins++;
                RedWins++;
            }

            MapFinished = true;
        }

        public void UserScoreReceived(string username, long score, bool passed)
        {
            _latestScores.Add(new Score(username, score, passed));
        }

#pragma warning disable CA1822 // Mark members as static
        public void SlotUpdated(Slot slot)
#pragma warning restore CA1822 // Mark members as static
        {
            return;
        }

        public void AllPlayersReady()
        {
            var slots = _slots.Values.Where(s => s.IsUsed);

            foreach (var slot in slots)
                slot.IsReady = true;

            OnAllPlayersReady?.Invoke(this, null);
        }

        public async Task<bool> WaitForAllPlayersReady(TimeSpan timeout)
        {
            long delta = 0;
            bool finished = false;

            OnAllPlayersReady += _OnAllPlayersReady_;

            while(!finished && delta < timeout.TotalMilliseconds)
            {
                await Task.Delay(50).ConfigureAwait(false);
                delta += 50;
            }

            if (finished)
                return true;
            else
            {
                OnAllPlayersReady -= _OnAllPlayersReady_;
                return false;
            }

            void _OnAllPlayersReady_(object sender, EventArgs e)
            {
                OnAllPlayersReady -= _OnAllPlayersReady_;
                finished = true;
            }
        }

        void MessageReceived(object sender, IrcChannelMessageEventArgs e)
        {
            ChatMessage msg = new ChatMessage(e.Sender, e.Message);

            for (int i = 0; i < ChatMessageActions.Count; i++)
            {
                if (ChatMessageActions[i].Invoke(msg))
                {
                    if (ChatMessageActions[i].RemoveOnSuccess)
                        ChatMessageActions.RemoveAt(i);

                    return;
                }
            }

            OnMessageReceived?.Invoke(this, msg);
        }

        void CreatedLobby(long matchId)
        {
            _irc.OnPrivateBanchoMessageReceived -= PrivateBanchoMessageReceived;
            _irc.OnChannelMessageReceived += MessageReceived;
            _settings.MatchId = matchId;
            IsLobbyClosed = false;
            CreationDate = DateTime.UtcNow;
            IsLobbyCreated = true;
            
            OnLobbyCreated?.Invoke(this, null);
        }

        void Tick()
        {
            while (_shouldTick)
            {
                OnBeforeTick?.Invoke(this, null);

                while (_tickQueue.TryDequeue(out Action a))
                {
                    if (!_shouldTick)
                        return;

                    try
                    {
                        a?.Invoke();
                    }
                    catch (Exception ex)
                    {
                        OnException?.Invoke(this, ex);
                    }
                }

                if (IsLobbyCreated)
                    OnAfterTick?.Invoke(this, null);

                Task.Delay(_tickDelay).ConfigureAwait(false).GetAwaiter().GetResult();
            }
        }

        void PrivateBanchoMessageReceived(object sender, IrcPrivateMessageEventArgs e)
        {
            const string _MP_START = "/mp/";
            if (!e.Message.StartsWith("created ", StringComparison.CurrentCultureIgnoreCase) ||
                !e.Message.EndsWith(Settings.RoomName, StringComparison.CurrentCultureIgnoreCase))
                return;

            int index = e.Message.IndexOf(_MP_START, StringComparison.CurrentCultureIgnoreCase);
            string msg = e.Message.Remove(0, index + _MP_START.Length);

            index = msg.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);

            if (!long.TryParse(msg.Substring(0, index), out long matchId))
            {
                Logger.Log("Failed to parse match id", LogLevel.Error);
                return;
            }

            CreatedLobby(matchId);
        }

        void CreateMatch()
        {
            _irc.OnPrivateBanchoMessageReceived += PrivateBanchoMessageReceived;
            _irc.SendMessageAsync("banchobot", $"!mp make {Settings.RoomName}").ConfigureAwait(false).GetAwaiter().GetResult();
        }

        void Close()
        {
            SendCommand(MPCommand.Close);
            IsLobbyClosed = true;
        }
    }

    public partial class LobbyController
    {
        /// <summary>
        /// Moves a player to another slot
        /// </summary>
        /// <param name="player">Player to move</param>
        /// <param name="slot">Slot to move player to</param>
        public void SetSlot(string player, int slot)
        {
            if (string.IsNullOrEmpty(player))
                throw new ArgumentNullException(nameof(player));
            else if (slot < 0)
                throw new ArgumentOutOfRangeException(nameof(slot));

            SendCommand(MPCommand.Move, player, slot);
        }

        /// <summary>
        /// Sets the current map
        /// </summary>
        /// <param name="map">Map id to set</param>
        /// <param name="mode">Gamemode</param>
        public void SetMap(long map, int? mode = null)
        {
            if (map <= 0)
                throw new ArgumentOutOfRangeException(nameof(map));

            if (mode.HasValue)
                SendCommand(MPCommand.Map, map, mode);
            else
                SendCommand(MPCommand.Map, map);
        }

        /// <summary>
        /// Sets the team of the player
        /// </summary>
        /// <param name="player">Player to change team for</param>
        /// <param name="color">Team to change to</param>
        public void SetTeam(string player, SlotColor color)
        {
            if (string.IsNullOrEmpty(player))
                throw new ArgumentNullException(nameof(player));

            SendCommand(MPCommand.Team, player, color.ToString().ToLower(CultureInfo.CurrentCulture));
        }

        /// <summary>
        /// Sets the current mods
        /// </summary>        
        /// <param name="mods">null for nomod</param>
        public void SetMods(string mods = null, bool freemod = false)
        {
            string modStr;
            if (mods == null)
            {
                if (freemod)
                    modStr = "Freemod";
                else
                    modStr = "None";
            }
            else
            {
                if (freemod)
                    modStr = $"{mods} Freemod";
                else
                    modStr = mods;
            }

            SendCommand(MPCommand.Mods, modStr);
        }

        /// <summary>
        /// Removes all mods and enables freemod
        /// </summary>
        public void SetFreemod()
        {
            SetMods("Freemod");
        }

        /// <summary>
        /// Set the lobby mods to nomod
        /// </summary>
        public void SetNomod()
        {
            SetMods("None");
        }

        /// <summary>
        /// Sets the win conditions
        /// </summary>
        /// <param name="teamMode">Team mode</param>
        /// <param name="condition">Win condition</param>
        /// <param name="slots">Amount of slots</param>
        public void SetLobby(TeamMode teamMode, WinCondition? condition, int? slots)
        {
            StringBuilder builder = new StringBuilder($"!mp set {(int)teamMode}");

            if (condition.HasValue)
                builder.Append($" {(int)condition}");
            if (slots.HasValue)
                builder.Append($" {slots}");

            SendCommand(MPCommand.Set, builder.ToString());
        }

        /// <summary>
        /// Sets the host
        /// </summary>
        /// <param name="nickname">Null to return host to the bot</param>
        public void SetHost(string nickname = null)
        {
            SendCommand(MPCommand.Host, nickname == null ? "clearhost" : nickname);
        }

        public void RequestSettings()
        {
            SendCommand(MPCommand.Settings);
        }

        /// <summary>
        /// Sends a command at the next tick
        /// </summary>
        /// <param name="cmd">Command to send</param>
        /// <param name="parameters">Command parameters</param>
        public void SendCommand(MPCommand cmd, params object[] parameters)
        {
            if (!IsLobbyCreated)
                throw new Exception("MP Lobby doesn't exist, cannot send mp command");

            StringBuilder cmdBuilder = new StringBuilder("!mp ");

            switch (cmd)
            {
                case MPCommand.None:
                    return;

                case MPCommand.CreateMatch:
                    cmdBuilder.Append("make");
                    break;

                default:
                    cmdBuilder.Append(cmd.ToString().ToLower(CultureInfo.CurrentCulture));
                    break;

            }

            if (parameters != null)
            {
                for (int i = 0; i < parameters.Length; i++)
                {
                    cmdBuilder.Append(' ');
                    cmdBuilder.Append(parameters[i].ToString());
                }
            }

            SendMessage(cmdBuilder.ToString());
        }

        /// <summary>
        /// Invites a player
        /// </summary>
        /// <param name="nickname">Nickname to invite</param>
        public void Invite(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
                throw new ArgumentNullException(nameof(nickname));

            SendCommand(MPCommand.Invite, nickname);
        }

        /// <summary>
        /// Starts the map after <paramref name="delay"/> seconds
        /// </summary>
        /// <param name="delay">Start delay, <see cref="TimeSpan.Zero"/> for default delay of 10 seconds</param>
        public void StartMap(TimeSpan delay)
        {
            if (delay.Equals(TimeSpan.Zero))
                delay = TimeSpan.FromSeconds(10);

            MapFinished = false;
            SendCommand(MPCommand.Start, (int)delay.TotalSeconds);
        }

        /// <summary>
        /// Starts a timer
        /// </summary>
        /// <param name="delay">Timer delay</param>
        public void StartTimer(TimeSpan delay)
        {
            if (delay.Equals(TimeSpan.Zero))
                throw new ArgumentNullException(nameof(delay));

            SendCommand(MPCommand.Timer, (int)delay.TotalSeconds);
        }

        /// <summary>
        /// Kicks a player
        /// </summary>
        /// <param name="player">Player to kick</param>
        public void Kick(string player)
        {
            if (string.IsNullOrEmpty(player))
                throw new ArgumentNullException(nameof(player));

            SendCommand(MPCommand.Kick, player);
        }

        /// <summary>
        /// Adds refs to the game
        /// </summary>
        /// <param name="players">Refs to add</param>
        public void AddRefs(params string[] players)
        {
            if (players == null || players.Length == 0)
                throw new ArgumentNullException(nameof(players));

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < players.Length; i++)
                builder.Append($"{players[i]} ");

            builder.Remove(builder.Length - 1, 1);

            SendCommand(MPCommand.AddRef, builder.ToString());
        }

        /// <summary>
        /// Removes refs from the game
        /// </summary>
        /// <param name="players">Refs to remove</param>
        public void RemoveRefs(params string[] players)
        {
            if (players == null || players.Length == 0)
                throw new ArgumentNullException(nameof(players));

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < players.Length; i++)
                builder.Append($"{players[i]} ");

            builder.Remove(builder.Length - 1, 1);

            SendCommand(MPCommand.RemoveRef, builder.ToString());
        }

        /// <summary>
        /// Lists all refs
        /// </summary>
        public void ListRefs()
        {
            SendCommand(MPCommand.ListRefs);
        }

        /// <summary>
        /// Aborts the currently running timer
        /// </summary>
        public void AbortTimer()
        {
            SendCommand(MPCommand.Aborttimer);
        }

        /// <summary>
        /// Adds a ref
        /// </summary>
        /// <param name="nickname">Ref to add</param>
        public void AddRef(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
                throw new ArgumentNullException(nameof(nickname));

            SendCommand(MPCommand.AddRef, nickname);
        }

        /// <summary>
        /// Removes a ref
        /// </summary>
        /// <param name="nickname">Ref to remove</param>
        public void RemoveRef(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
                throw new ArgumentNullException(nameof(nickname));

            SendCommand(MPCommand.RemoveRef, nickname);
        }

        /// <summary>
        /// Locks the match
        /// </summary>
        public void Lock()
        {
            SendCommand(MPCommand.Lock);
        }

        /// <summary>
        /// Unlocks the match
        /// </summary>
        public void Unlock()
        {
            SendCommand(MPCommand.Unlock);
        }

        /// <summary>
        /// Aborts the current map
        /// </summary>
        public void AbortMap()
        {
            SendCommand(MPCommand.Abort);
        }

        public void SendChannelMessage(string message)
        {
            SendMessage($"——— {message} ———");
        }

    }
}
