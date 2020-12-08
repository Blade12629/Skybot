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
using SkyBot.Osu.AutoRef.Events;
using AutoRefTypes.Extended.Requests;

namespace SkyBot.Osu.AutoRef
{
    public partial class LobbyController : ILobby
    {
        public ILobbySettings Settings { get => _settings; }
        public LobbyDataHandler DataHandler { get => _data; }

        List<ChatMessageAction> _chatMessageActions;
        List<ChatRequest> _requests;

        readonly LobbyDataHandler _data;
        readonly EventRunner _evRunner;
        readonly List<IrcChannelMessageEventArgs> _newMessages;

        Settings _settings;
        IRCClient _irc;

        public LobbyController(IRCClient irc, EventRunner evRunner)
        {
            _requests = new List<ChatRequest>();
            _newMessages = new List<IrcChannelMessageEventArgs>();
            _evRunner = evRunner;
            _chatMessageActions = ChatActions.ToList(this, evRunner);
            _settings = new Settings();
            _irc = irc;
            _data = new LobbyDataHandler(evRunner);
        }

        public void CreateLobby(string roomName)
        {
            _settings.Reset();
            _settings.RoomName = roomName;

            _irc.OnPrivateBanchoMessageReceived += PrivateBanchoMessageReceived;
            _irc.SendMessageAsync("banchobot", $"!mp make {Settings.RoomName}").ConfigureAwait(false).GetAwaiter().GetResult();

            _data.OnCreation(roomName);
        }

        public ILobbySettings GetSettings()
        {
            return Settings;
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

        public void RegisterRequest(ChatRequest request)
        {
            _requests.Add(request);
        }

        public void DebugLog(string msg)
        {
            Logger.Log(msg, LogLevel.Warning);
        }

        /// <summary>
        /// Sends a message at the next tick
        /// </summary>
        /// <param name="message">Message to send</param>
        void SendMessage(string message)
        {
            if (_data.Status == LobbyStatus.None ||
                _data.Status == LobbyStatus.Closed)
                throw new Exception("MP Lobby doesn't exist, cannot send mp command");

            _irc.SendMessageAsync(Settings.ChannelName, message).ConfigureAwait(false);
        }

        void MessageReceived(object sender, IrcChannelMessageEventArgs e)
        {
            _newMessages.Add(e);
        }

        public void ProcessIncomingMessages()
        {
            if (_newMessages.Count == 0)
                return;

            List<IrcChannelMessageEventArgs> messageArgs = _newMessages.ToList();
            _newMessages.Clear();

            foreach(var e in messageArgs)
            {
                ChatMessage msg = new ChatMessage(e.Sender, e.Message);

                for (int i = 0; i < _requests.Count; i++)
                {
                    if (_requests[i].RequestCancelled)
                    {
                        _requests.RemoveAt(i);
                        i--;
                        continue;
                    }

                    if (_requests[i].CheckStringCondition(msg))
                    {
                        _requests[i].Trigger(msg);
                        _requests[i].OnFinishRequest();
                        _requests.RemoveAt(i);
                        i--;
                        continue;
                    }
                }

                bool hasAction = false;

                for (int i = 0; i < _chatMessageActions.Count; i++)
                {
                    if (_chatMessageActions[i].Invoke(msg))
                    {
                        hasAction = true;

                        if (_chatMessageActions[i].RemoveOnSuccess)
                            _chatMessageActions.RemoveAt(i);

                        return;
                    }
                }

                if (!hasAction)
                    _evRunner.EnqueueEvent(EventHelper.CreateChatMessageEvent(msg));
            }
        }

        void CreatedLobby(ulong matchId)
        {
            _irc.OnPrivateBanchoMessageReceived -= PrivateBanchoMessageReceived;
            _irc.OnChannelMessageReceived += MessageReceived;
            _settings.MatchId = matchId;

            _data.OnCreated(matchId);
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

            if (!ulong.TryParse(msg.Substring(0, index), out ulong matchId))
            {
                Logger.Log("Failed to parse match id", LogLevel.Error);
                return;
            }

            CreatedLobby(matchId);
        }
    }

    //MP Commands
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
            if (_data.Status == LobbyStatus.None ||
                _data.Status == LobbyStatus.Closed)
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

        public void CloseLobby()
        {
            SendCommand(MPCommand.Close);
        }

        public void SendChannelMessage(string message)
        {
            SendMessage($"——— {message} ———");
        }
    }
}
