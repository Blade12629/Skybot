using AutoRefTypes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SkyBot.Osu.AutoRef.Data;
using SkyBot.Osu.AutoRef.Events;

namespace SkyBot.Osu.AutoRef.Chat
{
    public class ChatMessageAction
    {
        public bool RemoveOnSuccess { get; private set; }

        Func<ChatMessage, bool> _func;
        protected LobbyController _lc;
        protected EventRunner _evRunner;

        public ChatMessageAction(Func<ChatMessage, bool> func, bool removeOnSuccess, LobbyController lc, EventRunner eventRunner)
        {
            _func = func;
            RemoveOnSuccess = removeOnSuccess;
            _lc = lc;
            _evRunner = eventRunner;
        }

        protected ChatMessageAction(LobbyController lc, EventRunner eventRunner)
        {
            _lc = lc;
            _evRunner = eventRunner;
        }

        public virtual bool Invoke(ChatMessage message)
        {
            return _func?.Invoke(message) ?? false;
        }
    }

    public static class ChatActions
    {
        public static List<ChatMessageAction> ToList(LobbyController lc, EventRunner evRunner)
        {
            return new List<ChatMessageAction>()
            {
                new UserLeft(lc, evRunner),
                new UserJoined(lc, evRunner),
                new UserMoved(lc, evRunner),
                new UserScore(lc, evRunner),
                new UpdateSettings(lc, evRunner),
                new AllUsersReady(lc, evRunner),
                new MapFinished(lc, evRunner),
                new MatchStarted(lc, evRunner),
                new MapChanged(lc, evRunner),
                new MatchStartsIn(lc, evRunner),
                new QueueMatchStart(lc, evRunner),
                new AbortMatch(lc, evRunner)
            };
        }

        public class UserLeft : ChatMessageAction
#pragma warning restore CA1034 // Nested types should not be visible
        {
            public UserLeft(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase) ||
                    !message.Message.EndsWith("left the game", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                int index = message.Message.IndexOf("left the game", StringComparison.CurrentCultureIgnoreCase);
                string user = message.Message.Substring(0, index);

                _lc.DataHandler.OnUserLeaveLobby(user);

                return true;
            }
        }

        public class UserMoved : ChatMessageAction
        {
            public UserMoved(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase) ||
                    !message.Message.Contains("moved to slot", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                int index = message.Message.IndexOf("moved to slot", StringComparison.CurrentCultureIgnoreCase);
                string user = message.Message.Substring(0, index).TrimEnd(' ');

                index = message.Message.LastIndexOf(' ');
                int slot = int.Parse(message.Message.Remove(0, index + 1), CultureInfo.CurrentCulture);

                _lc.DataHandler.OnUserSwitchSlot(user, slot);

                return true;
            }
        }

        public class UserJoined : ChatMessageAction
        {
            public UserJoined(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase) ||
                    !message.Message.Contains("joined in slot", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                int index = message.Message.IndexOf(" joined in slot", StringComparison.CurrentCultureIgnoreCase);
                string user = message.Message.Substring(0, index);

                string msg = message.Message.Remove(0, index + "joined in slot".Length + 2);
                index = msg.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);

                int slot;
                if (index == -1)
                {
                    slot = int.Parse(msg.Remove(msg.Length - 1), CultureInfo.CurrentCulture);
                    _lc.DataHandler.OnUserJoinLobby(user, slot, SlotColor.None);
                    return true;
                }

                slot = int.Parse(msg.Substring(0, index), CultureInfo.CurrentCulture);

                index = msg.LastIndexOf(' ');
                msg = msg.Remove(0, index + 1).TrimEnd('.');

                if (char.IsLower(msg[0]))
                    msg = char.ToUpper(msg[0]) + msg.Remove(0, 1);

                SlotColor color = Enum.Parse<SlotColor>(msg);

                _lc.DataHandler.OnUserJoinLobby(user, slot, color);

                return true;
            }
        }

        public class UserScore : ChatMessageAction
        {
            const string _SCORE_TEXT = "finished playing (Score: ";

            public UserScore(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                int index = message.Message.IndexOf(_SCORE_TEXT, StringComparison.CurrentCultureIgnoreCase);

                if (index == -1)
                    return false;

                string username = message.Message.Substring(0, index - 1);
                string msg = message.Message.Remove(0, username.Length + 1 + _SCORE_TEXT.Length);

                index = msg.IndexOf(',', StringComparison.CurrentCultureIgnoreCase);

                string scoreStr = msg.Substring(0, index);
                string passedStr = msg.Remove(0, index + 1).TrimEnd('.').TrimEnd(')');

                long score = int.Parse(scoreStr, CultureInfo.CurrentCulture);
                bool passed = passedStr.Equals("passed", StringComparison.CurrentCultureIgnoreCase);

                _lc.DataHandler.OnReceiveScore(username, score, passed);

                return true;
            }
        }

        public class UpdateSettings : ChatMessageAction
        {
            const string _SLOT = "slot";
            const string _READY = "Ready";
            const string _NOT_READY = "Not Ready";
            const string _TEAM_RED = "Team Red";
            const string _TEAM_BLUE = "Team Blue";
            const string _SPLITTER = " / ";
            const string _HOST_ROLE = "Host";

            Dictionary<string, Action<string>> _settingParsers;

            public UpdateSettings(LobbyController lc, EventRunner evr) : base(lc, evr)
            {
                _settingParsers = new Dictionary<string, Action<string>>()
                {
                    { "room name: ", new Action<string>(s => ((Settings)lc.Settings).RoomName = s) },
                    { "history: ", new Action<string>(s => ((Settings)lc.Settings).HistoryUrl = s) },
                    { "team mode: ", new Action<string>(s => ((Settings)lc.Settings).TeamMode = Enum.Parse<TeamMode>(s)) },
                    { "win condition: ", new Action<string>(s => ((Settings)lc.Settings).WinCondition = Enum.Parse<WinCondition>(s)) }
                };
            }

            public override bool Invoke(ChatMessage message)
            {
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                string line = message.Message;

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

                    SlotColor color = SlotColor.None;
                    string role = null;
                    List<string> mods = new List<string>();
                    if (index > -1)
                    {
                        line = line.Remove(0, index + 1);
                        if (line.StartsWith(_TEAM_BLUE, StringComparison.CurrentCultureIgnoreCase))
                        {
                            color = SlotColor.Blue;
                            line = line.Remove(0, _TEAM_BLUE.Length);
                        }
                        else if (line.StartsWith(_TEAM_RED, StringComparison.CurrentCultureIgnoreCase))
                        {
                            color = SlotColor.Red;
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

                    Slot slot = (Slot)_lc.DataHandler.GetSlot(slotId);
                    slot.Reset();

                    slot.IsReady = isReady;
                    slot.ProfileUrl = new Uri(profileUrl);
                    slot.Nickname = nickname;
                    slot.Role = role;
                    slot.Color = color;

                    if (mods.Count > 0)
                        slot.Mods.AddRange(mods);

                    _lc.DataHandler.OnSlotUpdate(slot);

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

                    pair.Value(value);
                }

                return true;
            }

            static string TryParseSetting(ref string input, string token)
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
        }

        public class AllUsersReady : ChatMessageAction
        {
            public AllUsersReady(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase) ||
                    !message.Message.Equals("All players are ready", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                _lc.DataHandler.OnAllPlayersReady();

                return true;
            }
        }

        public class MapFinished : ChatMessageAction
        {
            public MapFinished(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                return false;
            }
        }

        public class MatchStarted : ChatMessageAction
        {
            public MatchStarted(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                if (message.Message.Equals("The match has started!", StringComparison.CurrentCultureIgnoreCase))
                {
                    _lc.DataHandler.OnMapStart();
                    return true;
                }
                else if (message.Message.Equals("started the match", StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }

                return false;
            }
        }

        public class MapChanged : ChatMessageAction
        {
            public MapChanged(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                //Changed beatmap to https://osu.ppy.sh/b/1591935 Asriel - ABYSS
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                if (message.Message.StartsWith("changed beatmap", StringComparison.CurrentCultureIgnoreCase))
                {
                    string[] split = message.Message.Split(' ');
                    //https://osu.ppy.sh/b/1591935
                    string url = split[3];
                    //1591935
                    string idStr = url.Split('/').Last();

                    ulong mapId = ulong.Parse(idStr);
                    _lc.DataHandler.OnMapChange(mapId);
                }
                else if (message.Message.StartsWith("beatmap changed to:", StringComparison.CurrentCultureIgnoreCase))
                {
                    string id = message.Message.Split('/').Last().TrimEnd(')');
                    _lc.DataHandler.OnMapChange(ulong.Parse(id));
                }
                else if (message.Message.Equals("Host is changing map...", StringComparison.CurrentCultureIgnoreCase))
                {
                    _lc.DataHandler.OnMapChange(0);
                }
                else
                    return false;

                return true;
            }
        }

        public class MatchStartsIn : ChatMessageAction
        {
            public MatchStartsIn(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                //BanchoBot: Match starts in 1 second
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase) ||
                    !message.Message.StartsWith("match starts in", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                string secondsStr = message.Message.Split(' ')[3];
                _lc.DataHandler.OnMatchStartsIn(long.Parse(secondsStr));

                return true;
            }
        }

        public class QueueMatchStart : ChatMessageAction
        {
            public QueueMatchStart(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                //BanchoBot: Queued the match to start in 1 second
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase) ||
                    !message.Message.StartsWith("queued the match to start", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                string secondsStr = message.Message.Split(' ')[6];
                _lc.DataHandler.OnQueueMatchStart(long.Parse(secondsStr));

                return true;
            }
        }

        public class AbortMatch : ChatMessageAction
        {
            public AbortMatch(LobbyController lc, EventRunner evr) : base(lc, evr)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                //Changed beatmap to https://osu.ppy.sh/b/1591935 Asriel - ABYSS
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase) ||
                    !message.Message.Equals("aborted the match", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                _lc.DataHandler.OnAbortMatch();

                return true;
            }
        }

        public class ChangeHost : ChatMessageAction
        {
            public ChangeHost(LobbyController lc, EventRunner ev) : base(lc, ev)
            {

            }

            public override bool Invoke(ChatMessage message)
            {
                if (!message.From.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase))
                    return false;

                if (message.Message.StartsWith("Changed match host to", StringComparison.CurrentCultureIgnoreCase))
                    return true;
                else if (message.Message.EndsWith("became the host", StringComparison.CurrentCultureIgnoreCase))
                {
                    string user = message.Message.Split(' ')[0];
                    _lc.DataHandler.OnChangeHost(user);
                    return true;
                }

                return false;
            }
        }

    }
}
