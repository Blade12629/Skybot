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

namespace SkyBot.Osu.AutoRef
{
    public class LobbyController
    {
        public event EventHandler OnBeforeCreating;
        public event EventHandler OnBeforeClosing;
        public event EventHandler OnBeforeTick;
        public event EventHandler OnAfterTick;
        public event EventHandler<Exception> OnException;
        public event EventHandler OnAllPlayersReady;

        public event EventHandler OnCreated;
        public event EventHandler<ChatMessage> OnMessageReceived;

        public bool IsClosed { get; private set; }
        public Settings Settings { get; private set; }
        public IReadOnlyDictionary<int, Slot> Slots => _slots;
        public IReadOnlyList<Score> Scores => _totalScores;
        public List<Score> LatestScores => _latestScores;
        public List<ChatMessageAction> ChatMessageActions { get; private set; }

        Dictionary<int, Slot> _slots;
        List<Score> _totalScores;
        List<Score> _latestScores;

        IRCClient _irc;

        Task _tickTask;
        CancellationTokenSource _tickSource;
        bool _shouldTick;
        int _tickDelay;
        ConcurrentQueue<Action> _tickQueue;

        public LobbyController(IRCClient irc, int tickDelay = 100)
        {
            ChatMessageActions = ChatActions.ToList(this);
            Settings = new Settings();
            IsClosed = true;
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

            Settings.Reset();
            Settings.RoomName = roomName;
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

        #region MP Commands
        /// <summary>
        /// Sends a command at the next tick
        /// </summary>
        /// <param name="cmd">Command to send</param>
        /// <param name="parameters">Command parameters</param>
        public void SendCommand(MPCommand cmd, params object[] parameters)
        {
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
        /// Moves a player to another slot
        /// </summary>
        /// <param name="player">Player to move</param>
        /// <param name="slot">Slot to move player to</param>
        public void MovePlayer(string player, int slot)
        {
            SendCommand(MPCommand.Move, player, slot);
        }

        /// <summary>
        /// Invites a player
        /// </summary>
        /// <param name="nickname">Nickname to invite</param>
        public void Invite(string nickname)
        {
            SendCommand(MPCommand.Invite, nickname);
        }

        /// <summary>
        /// Sets the current map
        /// </summary>
        /// <param name="map">Map id to set</param>
        /// <param name="mode">Gamemode</param>
        public void SetMap(long map, int mode = 0)
        {
            SendCommand(MPCommand.Map, map, mode);
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
        /// Adds a ref
        /// </summary>
        /// <param name="nickname">Ref to add</param>
        public void AddRef(string nickname)
        {
            SendCommand(MPCommand.AddRef, nickname);
        }

        /// <summary>
        /// Removes a ref
        /// </summary>
        /// <param name="nickname">Ref to remove</param>
        public void RemoveRef(string nickname)
        {
            SendCommand(MPCommand.RemoveRef, nickname);
        }

        /// <summary>
        /// Locks the match
        /// </summary>
        public void LockMatch()
        {
            SendCommand(MPCommand.Lock);
        }

        /// <summary>
        /// Unlocks the match
        /// </summary>
        public void UnlockMatch()
        {
            SendCommand(MPCommand.Unlock);
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

        /// <summary>
        /// Aborts the current map
        /// </summary>
        public void AbortMap()
        {
            SendCommand(MPCommand.Abort);
        }

        public void RefreshSettings()
        {
            SendCommand(MPCommand.Settings);
        }

        void CreateMatch()
        {
            _irc.OnPrivateBanchoMessageReceived += PrivateBanchoMessageReceived;
            _irc.SendMessageAsync("banchobot", $"!mp make {Settings.RoomName}").ConfigureAwait(false).GetAwaiter().GetResult();
        }

        void Close()
        {
            SendCommand(MPCommand.Close);
            IsClosed = true;
        }
        #endregion

        /// <summary>
        /// Sends a message at the next tick
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(string message)
        {
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

        public void UserJoined(string user, int slot, SlotColor? team)
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

        public void UserScore(string username, long score, bool passed)
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

        /// <summary>
        /// Sorts players, this requires a max of 10 players and no players above slot 10
        /// </summary>
        /// <param name="players"></param>
        /// <param name="slotStart"></param>
        public void SortTeams(List<string> teamA, string teamACap, List<string> teamB, string teamBCap, int playersPerTeam)
        {
            int nextFreeSlot = 11;

            Slot slot1 = _slots[1];

            //Move player away and captain to slot
            if (slot1.IsUsed && !slot1.Nickname.Equals(teamACap, StringComparison.CurrentCultureIgnoreCase))
            {
                Move(slot1.Nickname);
                MovePlayer(teamACap, 0);
            }
            //Move captain if not in slot
            else
                MovePlayer(teamACap, 0);

            for (int i = 0; i < teamA.Count; i++)
            {
                Slot slot = _slots[i + 2];

                if (!slot.IsUsed)
                {
                    MovePlayer(teamA[i], slot.Id);
                }
                else if (!slot.Nickname.Equals(teamA[i], StringComparison.CurrentCultureIgnoreCase))
                {
                    Move(slot.Nickname);
                    MovePlayer(teamA[i], slot.Id);
                }
            }

            int capRedSlotId = playersPerTeam + 1;
            Slot slot2 = _slots[capRedSlotId];

            //Move player away and captain to slot
            if (slot2.IsUsed && !slot1.Nickname.Equals(teamBCap, StringComparison.CurrentCultureIgnoreCase))
            {
                Move(slot2.Nickname);
                MovePlayer(teamBCap, capRedSlotId);
            }
            //Move captain if not in slot
            else
                MovePlayer(teamBCap, capRedSlotId);

            //Get wrong slots
            List<string> playersRed = teamB.ToList();
            List<int> freeSlots = new List<int>();

            for (int id = capRedSlotId + 1; id < playersPerTeam * 2; id++)
            {
                Slot slot = _slots[id];

                if (slot.IsUsed)
                    playersRed.Remove(slot.Nickname);
                else
                    freeSlots.Add(slot.Id);
            }

            for (int i = 0; i < freeSlots.Count; i++)
            {
                MovePlayer(playersRed[0], freeSlots[i]);
                playersRed.RemoveAt(0);
            }

            void Move(string player)
            {
                MovePlayer(player, nextFreeSlot);
                nextFreeSlot++;
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

        void MatchCreated(long matchId)
        {
            _irc.OnPrivateBanchoMessageReceived -= PrivateBanchoMessageReceived;
            _irc.OnChannelMessageReceived += MessageReceived;
            Settings.MatchId = matchId;
            IsClosed = false;

            OnCreated?.Invoke(this, null);
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

            MatchCreated(matchId);
        }
    }

}
