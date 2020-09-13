using SkyBot.Networking.Irc;
using SkyBot.Osu.IRC;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using IRCClient = SkyBot.Osu.IRC.OsuIrcClient;
using System.Threading.Tasks;

namespace SkyBot.Osu.AutoRef
{
    public class LobbyController
    {
        public event EventHandler OnLobbyCreated;
        /// <summary>
        /// (username, score, passed)
        /// </summary>
        public event EventHandler<LobbyScore> OnScoreReceived;
        public event EventHandler<LobbySlot> OnSlotUpdated;
        public event EventHandler OnSettingUpdated;

        public bool IsClosed { get; private set; }
        public LobbySetting Settings => _settings.Copy();
        public IReadOnlyDictionary<int, LobbySlot> Slots => _slots;
        public bool RefreshedSettings { get; private set; }

        private IRCClient _irc;
        private string _tempMatchName;

        private LobbySetting _settings;
        private readonly object _settingsLock = new object();

        private ConcurrentDictionary<int, LobbySlot> _slots;

        private Dictionary<string, Action<string>> _settingParsers;
        private ConcurrentQueue<ChatInteraction> _chatInteractions;

        public LobbyController(IRCClient irc)
        {
            _irc = irc;
            _chatInteractions = new ConcurrentQueue<ChatInteraction>();
            _settings = new LobbySetting();
            _slots = new ConcurrentDictionary<int, LobbySlot>();

            for (int i = 0; i < 16; i++)
                _slots.TryAdd(i + 1, new LobbySlot(i + 1));

            _settingParsers = new Dictionary<string, Action<string>>()
            {
                { "room name: ", new Action<string>(s => _settings.RoomName = s) },
                { "history: ", new Action<string>(s => _settings.HistoryUrl = s) },
                { "team mode: ", new Action<string>(s => _settings.TeamMode = Enum.Parse<TeamMode>(s)) },
                { "win condition: ", new Action<string>(s => _settings.WinCondition = Enum.Parse<WinCondition>(s)) }
            };
        }

        public void RefreshSettings()
        {
            RefreshedSettings = false;

            for (int i = 0; i < 16; i++)
                _slots[i + 1].Reset();

            SendChannelMessage("!mp settings");
        }

        public void SetTeam(string username, LobbyColor team)
        {
            SendChannelMessage($"!mp team {username} {team.ToString().ToLower(CultureInfo.CurrentCulture)}");
        }

        public void SetSlot(string username, int slot)
        {
            SendChannelMessage($"!mp move {username} {slot}");
        }

        public void CreateMatch(string matchName)
        {
            lock(_settings)
            {
                _settings.Reset();
                IsClosed = false;
            }

            _tempMatchName = matchName;
            _irc.OnPrivateBanchoMessageReceived += ReadPrivBanchoMessage;

            _irc.SendMessageAsync("banchobot", $"!mp make {matchName}").ConfigureAwait(false);
        }

        public void SendChannelMessage(string message)
        {
            if (IsClosed)
                return;

            _irc.SendMessageAsync(_settings.ChannelName, message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void CloseMatch()
        {
            if (IsClosed)
                return;

            _irc.OnChannelMessageReceived -= ReadChannelMessage;
            SendChannelMessage("!mp close");
            IsClosed = true;
        }

        public void Invite(string nickname)
        {
            SendChannelMessage($"!mp invite {nickname}");
        }

        public void SetMap(long map, int mode = 0)
        {
            SendChannelMessage($"!mp map {map} {mode}");
        }

        /// <param name="mods">null for nomod</param>
        public void SetMods(string mods = null)
        {
            SendChannelMessage($"!mp mods {(mods == null ? "None" : mods)}");
        }

        public void SetFreemod()
        {
            SetMods("Freemod");
        }

        public void SetNomod()
        {
            SetMods("None");
        }

        public void AddRef(string nickname)
        {
            SendChannelMessage($"!mp addref {nickname}");
        }

        public void RemoveRef(string nickname)
        {
            SendChannelMessage($"!mp removeref {nickname}");
        }

        public void SetMatchLock(bool locked)
        {
            SendChannelMessage($"!mp {(locked ? "lock" : "unlock")}");
        }

        public void SetWinConditions(TeamMode teamMode, WinCondition? condition, int? slots)
        {
            StringBuilder builder = new StringBuilder($"!mp set {(int)teamMode}");

            if (condition.HasValue)
                builder.Append($" {(int)condition}");
            if (slots.HasValue)
                builder.Append($" {slots}");

            SendChannelMessage(builder.ToString());
        }

        /// <param name="nickname">Null to return host to the bot</param>
        public void SetHost(string nickname = null)
        {
            SendChannelMessage($"!mp host {(nickname == null ? "clearhost" : nickname)}");
        }

        public void AbortMatch()
        {
            SendChannelMessage("!mp abort");
        }

        public int GetSlotForUser(string nickname)
        {
            if (string.IsNullOrEmpty(nickname))
                return -1;

            var pair = _slots.FirstOrDefault(p => p.Value.Nickname.Equals(nickname, StringComparison.CurrentCultureIgnoreCase));

            if (pair.Value == null)
                return -1;

            return pair.Key;
        }

        public LobbyRoll RequestRoll(string from)
        {
            const string _ROLL_CMD = "!roll";

            int rolled = -1;
            int min = 0;
            int max = 100;

            bool doneFirst = false;
            bool doneSecond = false;

            RequestChatInteraction(from, _ROLL_CMD, s =>
            {
                s = s.Remove(0, _ROLL_CMD.Length).TrimStart(' ');

                if (s.Length == 0)
                {
                    doneFirst = true;
                    return;
                }

                int index = s.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);

                if (index > -1)
                {
                    if (int.TryParse(s.Substring(0, index), out int min_))
                        min = min_;
                    if (int.TryParse(s.Remove(0, index + 1), out int max_))
                        max = max_;
                }
                else if (int.TryParse(s, out int max_))
                    max = max_;

                doneFirst = true;
            });

            RequestChatInteraction("banchobot", $"{from} rolls", new Action<string>(s =>
            {
                string[] split = s.Remove(0, from.Length + 1).Split(' ');
                rolled = int.Parse(split[1], CultureInfo.CurrentCulture);

                doneSecond = true;
            }));

            SendChannelMessage($"{from} please roll via !roll");

            while (!doneSecond && !doneFirst)
                Task.Delay(1).ConfigureAwait(false).GetAwaiter().GetResult();

            return new LobbyRoll(from, min, max, rolled);
        }

        public long? RequestPick(string from, string message = null)
        {
            bool done = false;
            long? mapId = null;

            RequestChatInteraction(from, "!pick ", new Action<string>(s =>
            {
                int index = s.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);

                if (index == -1)
                {
                    done = true;
                    return;
                }

                string msg = s.Remove(0, index + 1);

                if (!long.TryParse(msg, out long beatmapId))
                {
                    done = true;
                    return;
                }

                mapId = beatmapId;
                done = true;
            }));

            if (!string.IsNullOrEmpty(message))
                SendChannelMessage(message);

            while (!done)
                Task.Delay(1).ConfigureAwait(false).GetAwaiter().GetResult();

            return mapId;
        }


        public void RequestChatInteraction(string nickname, string messageStart, Action<string> action)
        {
            _chatInteractions.Enqueue(new ChatInteraction(nickname, messageStart, action));
        }

        public void RequestAndWaitForChatInteraction(string nickname, string messageStart, Action<string> action)
        {
            object token = false;

            Action<string> newAction = new Action<string>(s =>
            {
                action(s);
                token = true;
            });

            _chatInteractions.Enqueue(new ChatInteraction(nickname, messageStart, newAction));

            while (!(bool)token)
                Task.Delay(1).ConfigureAwait(false).GetAwaiter().GetResult();
        }



        private void OnMatchCreated(long matchId, string matchName)
        {
            _irc.OnPrivateBanchoMessageReceived -= ReadPrivBanchoMessage;
            _irc.OnChannelMessageReceived += ReadChannelMessage;

            lock (_settingsLock)
            {
                _settings.MatchId = matchId;
                _settings.RoomName = matchName;
            }

            OnLobbyCreated?.Invoke(this, new EventArgs());
        }

        private static string TryParseSetting(ref string input, string token)
        {
            int index = input.IndexOf(token, StringComparison.CurrentCultureIgnoreCase);

            if (index == -1)
                return null;

            input = input.Remove(0, index + token.Length);
            index = input.IndexOf(',', StringComparison.CurrentCultureIgnoreCase);

            string result = input;

            if (index > -1)
            {
                result = result.Substring(0, index);
                input = input.Remove(0, index + 1);
            }

            return result;
        }

        private bool TryUpdateSetting(string line)
        {
            const string _SLOT = "slot";
            const string _READY = "Ready";
            const string _NOT_READY = "Not Ready";
            const string _TEAM_RED = "Team Red";
            const string _TEAM_BLUE = "Team Blue";
            const string _SPLITTER = " / ";
            const string _HOST_ROLE = "Host";

            if (line.StartsWith(_SLOT, StringComparison.CurrentCultureIgnoreCase))
            {
                line = line.Remove(0, _SLOT.Length).TrimStart(' ');

                int slotId = 0;
                int slotIdLength = 1;

                if (char.IsNumber(line[1]))
                    slotIdLength = 2;

                slotId = int.Parse(line.Substring(0, slotIdLength), CultureInfo.CurrentCulture);
                line = line.Remove(0, slotIdLength).TrimStart(' ');

                bool isReady = false;
                if (line.StartsWith(_READY, StringComparison.CurrentCultureIgnoreCase))
                {
                    isReady = true;
                    line = line.Remove(0, _READY.Length).TrimStart(' ');
                }
                else
                    line = line.Remove(0, _NOT_READY.Length).TrimStart(' ');

                int index = line.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);
                string profileUrl = line.Substring(0, index);
                line = line.Remove(0, index + 1);

                index = line.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);
                string nickname = line.Substring(0, index).Replace(' ', '_');
                line = line.Remove(0, index).TrimStart(' ');

                index = line.IndexOf('[', StringComparison.CurrentCultureIgnoreCase);

                string role = null;
                List<string> mods = new List<string>();
                if (index > -1)
                {
                    line = line.Remove(0, index + 1);

                    LobbyColor color = LobbyColor.None;
                    if (line.StartsWith(_TEAM_BLUE, StringComparison.CurrentCultureIgnoreCase))
                    {
                        color = LobbyColor.Blue;
                        line = line.Remove(0, _TEAM_BLUE.Length);
                    }
                    else if (line.StartsWith(_TEAM_RED, StringComparison.CurrentCultureIgnoreCase))
                    {
                        color = LobbyColor.Red;
                        line = line.Remove(0, _TEAM_RED.Length);
                    }

                    if (line.StartsWith(_SPLITTER, StringComparison.CurrentCultureIgnoreCase))
                        line = line.Remove(0, _SPLITTER.Length);

                    if (line.StartsWith(_HOST_ROLE, StringComparison.CurrentCultureIgnoreCase))
                    {
                        role = _HOST_ROLE;
                        line = line.Remove(0, _HOST_ROLE.Length);
                    }

                    if (line.StartsWith(_SPLITTER, StringComparison.CurrentCultureIgnoreCase))
                        line = line.Remove(0, _SPLITTER.Length);

                    string[] split;
                    if (line.Contains(',', StringComparison.CurrentCultureIgnoreCase))
                        split = line.TrimEnd(']').Split(',');
                    else
                        split = new string[1] { line.TrimEnd(']') };

                    mods.AddRange(split);
                }

                LobbySlot slot = _slots[slotId];

                slot.IsReady = isReady;
                slot.ProfileUrl = profileUrl;
                slot.Nickname = nickname;
                slot.Role = role;

                if (slot.Mods.Count > 0)
                    slot.Mods.Clear();

                if (mods.Count > 0)
                    slot.Mods.AddRange(mods);

                OnSlotUpdated?.Invoke(this, slot);
                return true;
            }

            for (int i = 0; i < 2; i++)
            {
                var pair = _settingParsers.FirstOrDefault(p => line.StartsWith(p.Key, StringComparison.CurrentCultureIgnoreCase));

                if (pair.Key == null || pair.Value == null)
                {
                    if (i == 1)
                        return true;

                    return false;
                }

                string value = TryParseSetting(ref line, pair.Key);
                line = line.TrimStart(' ');

                if (value == null)
                {
                    if (i == 1)
                        return true;

                    return false;
                }

                lock (_settingsLock)
                {
                    pair.Value(value);
                }
            }

            OnSettingUpdated?.Invoke(this, new EventArgs());
            return true;
        }

        private bool TryReadScore(string line)
        {
            const string _SCORE_TEXT = "finished playing (Score: ";

            int index = line.IndexOf(_SCORE_TEXT, StringComparison.CurrentCultureIgnoreCase);

            if (index == -1)
                return false;

            string username = line.Substring(0, index - 1);
            string msg = line.Remove(0, username.Length + 1 + _SCORE_TEXT.Length);

            index = msg.IndexOf(',', StringComparison.CurrentCultureIgnoreCase);

            string scoreStr = msg.Substring(0, index);
            string passedStr = msg.Remove(0, index + 1).TrimEnd('.').TrimEnd(')');

            long score = int.Parse(scoreStr, CultureInfo.CurrentCulture);
            bool passed = passedStr.Equals("passed", StringComparison.CurrentCultureIgnoreCase);

            OnScoreReceived?.Invoke(this, new LobbyScore(username, score, passed));

            return true;
        }

        private void ReadPrivBanchoMessage(object sender, IrcPrivateMessageEventArgs args)
        {
            const string _MP_START = "/mp/";
            if (!args.Message.StartsWith("created ", StringComparison.CurrentCultureIgnoreCase) ||
                !args.Message.EndsWith(_tempMatchName, StringComparison.CurrentCultureIgnoreCase))
                return;

            int index = args.Message.IndexOf(_MP_START, StringComparison.CurrentCultureIgnoreCase);
            string msg = args.Message.Remove(0, index + _MP_START.Length);

            index = msg.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);
            
            if (!long.TryParse(msg.Substring(0, index), out long matchId))
            {
                Logger.Log("Failed to parse match id", LogLevel.Error);
                return;
            }

            OnMatchCreated(matchId, _tempMatchName);
            _tempMatchName = null;
        }

        private void ReadChannelMessage(object sender, IrcChannelMessageEventArgs args)
        {
            Logger.Log($"Message from {args.Sender} to {args.Destination}: {args.Message}");

            for (int i = 0; i < _chatInteractions.Count; i++)
            {
                if (!_chatInteractions.TryDequeue(out ChatInteraction action))
                    break;

                if (action.Nickname.Equals(args.Sender, StringComparison.CurrentCultureIgnoreCase) &&
                    args.Message.StartsWith(action.MessageStart, StringComparison.CurrentCultureIgnoreCase))
                {
                    action.Action(args.Message);
                    return;
                }

                _chatInteractions.Enqueue(action);
            }

            if (args.Sender.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase))
            {
                ReadChannelBanchoMessage(sender, args);
                return;
            }
        }

        private void ReadChannelBanchoMessage(object sender, IrcChannelMessageEventArgs args)
        {
            if (TryUpdateSetting(args.Message) || TryReadScore(args.Message))
                return;

        }
    }
}
