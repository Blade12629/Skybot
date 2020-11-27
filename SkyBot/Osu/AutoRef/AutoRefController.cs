using SkyBot.Osu.AutoRef.Chat;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyBot.Osu.AutoRef
{
    /*
        new Action(DisableSettingsUpdate),
        new Action(InitSetup),

        new Action(WaitForLobbyInvites),

        new Action(InvitePlayers),
        new Action(WaitForPlayersToJoin),
        new Action(_controller.RefreshSettings),
        new Action(() => WaitFor(DateTime.UtcNow.AddSeconds(5))),
        new Action(SortPlayers),
        new Action(SetTeamColors),
        new Action(WaitForMatchStartTime),
        new Action(() => SendMessage("Welcome to Skybot's auto-ref (Alpha 0.1.1)")),
        new Action(() => SendMessage($"Blue captain: {_captainBlueDisplay} | Red captain: {_captainRedDisplay}")),
        new Action(Setup),

        new Action(() => WaitFor(DateTime.UtcNow.AddSeconds(2))),
        new Action(GetBans),

        new Action(PlayPhase),

        new Action(SubmitResults),
        new Action(CloseLobby)
     */
    public class AutoRefController
    {
        public LobbyController LC => _lc;
        public AutoRefSettings Settings { get; set; }

        public bool LastRollValid => !_lastRollInvalid;
        public Roll LastRoll => _lastRoll;

        public bool LastPickValid => !_lastPickInvalid;
        public long LastPick => _lastPick;

        public int WorkflowState { get; set; }

        public SlotColor CurrentCaptain { get; set; }

        public List<long> BannedMaps { get; set; }
        public List<long> PickedMaps { get; set; }
        public List<long> Mappool { get; set; }
        public long TieBreaker { get; set; }

        public int BlueWins => LC.BlueWins;
        public int RedWins => LC.RedWins;

        LobbyController _lc;
        List<Func<bool>> _tickQueue;
        Queue<ChatInteraction> _interactionsQueue;

        bool _lastRollInvalid;
        Roll _lastRoll;

        long _lastPick;
        bool _lastPickInvalid;

        int _rollRequest;
        Roll _blueRoll;
        Roll _redRoll;

        public AutoRefController(LobbyController lc)
        {
            _tickQueue = new List<Func<bool>>();
            _lc = lc;
            _lc.OnAfterTick += Tick;
            _lc.OnMessageReceived += OnMessageReceived;
        }

        public void AddTicks(List<Func<bool>> ticks)
        {
            _tickQueue.AddRange(ticks);
        }

        public void Start(string lobbyName)
        {
            _tickQueue = new List<Func<bool>>();
            _interactionsQueue = new Queue<ChatInteraction>();
            _lc.CreateLobby(lobbyName);
        }

        public void TestRun()
        {
            for (int i = 0; i < _tickQueue.Count; i++)
                _tickQueue[i].Invoke();
        }

        public void RequestResponse(string from, string startsWith, Action<string> a)
        {
            _interactionsQueue.Enqueue(new ChatInteraction(from, startsWith, a));
        }

        /// <summary>
        /// Requests a roll via !roll
        /// </summary>
        /// <param name="from">User to request roll from</param>
        public void RequestRoll(string from)
        {
            _lastRollInvalid = false;
            _lastRoll = null;

            RequestResponse(from, "!roll ", new Action<string>(s =>
            {
                string[] split = s.Split(' ');

                if (split != null && split.Length > 1)
                {
                    if (long.TryParse(split[1], out long min))
                    {
                        if (split.Length >= 3 && long.TryParse(split[2], out long max))
                        {
                            if (min != 0 || 
                                max != 100)
                                _lastRollInvalid = true;
                        }
                        else if (min != 100)
                            _lastRollInvalid = true;
                    }
                }
            }));

            RequestResponse("banchobot", $"{from} rolls", new Action<string>(s =>
            {
                string[] split = s.Remove(0, from.Length + 1).Split(' ');
                int rolled = int.Parse(split[1], CultureInfo.CurrentCulture);

                _lastRoll = new Roll(from, 0, 100, rolled);
            }));

            SendMessage($"{from} please roll via !roll");
        }

        public void SendMessage(string message)
        {
            _lc.SendMessage($"——— {message} ———");
        }

        public bool RequestRolls()
        {
            switch (_rollRequest)
            {
                default:
                case 0:
                    RequestRoll(Settings.CaptainBlue);
                    _rollRequest = 1;
                    return false;

                case 1:
                    if (_lastRoll == null)
                        return false;
                    else if (_lastRollInvalid)
                        goto case 0;

                    _blueRoll = _lastRoll;
                    _rollRequest = 2;

                    return false;

                case 2:
                    RequestRoll(Settings.CaptainRed);
                    _rollRequest = 3;
                    return false;

                case 3:
                    if (_lastRoll == null)
                        return false;
                    else if (_lastRollInvalid)
                        goto case 2;
                    else if (_lastRoll.Rolled == _blueRoll.Rolled)
                    {
                        _rollRequest = 0;
                        goto case 0;
                    }

                    _redRoll = _lastRoll;
                    _rollRequest = 0;

                    return true;
            }
        }

        public SlotColor GetRollWinColor()
        {
            if (_blueRoll == null ||
                _redRoll == null)
                return SlotColor.Blue;

            return _blueRoll.Rolled > _redRoll.Rolled ? SlotColor.Blue : SlotColor.Red;
        }

        /// <summary>
        /// Requests a pick via !pick mapId
        /// </summary>
        /// <param name="from">User to request pick from</param>
        public void RequestPick(string from)
        {
            _lastPick = 0;
            _lastPickInvalid = false;

            RequestResponse(from, "!pick ", new Action<string>(s =>
            {
                string[] split = s.Remove(0, from.Length + 1).Split(' ');

                if (split == null || split.Length < 2)
                {
                    _lastPickInvalid = true;
                    return;
                }

                if (!int.TryParse(split[1], out int rolled))
                {
                    _lastPickInvalid = true;
                    return;
                }

                _lastPick = rolled;
            }));

            SendMessage($"{from} please pick a map via !pick mapId");
        }

        /// <summary>
        /// Sorts the players based on <see cref="Settings"/>
        /// </summary>
        public void SortPlayers()
        {
            SortTeams(Settings.PlayersBlue, Settings.CaptainBlue, Settings.PlayersRed, Settings.CaptainRed, Settings.PlayersPerTeam);
        }

        /// <summary>
        /// Sorts players, this requires a max of 10 players and no players above slot 10
        /// </summary>
        /// <param name="players"></param>
        /// <param name="slotStart"></param>
        public void SortTeams(List<string> teamA, string teamACap, List<string> teamB, string teamBCap, int playersPerTeam)
        {
            int nextFreeSlot = 11;

            Slot slot1 = _lc.Slots[1];

            //Move player away and captain to slot
            if (slot1.IsUsed && !slot1.Nickname.Equals(teamACap, StringComparison.CurrentCultureIgnoreCase))
            {
                Move(slot1.Nickname);
                _lc.MovePlayer(teamACap, 0);
                _lc.SetTeam(teamACap, SlotColor.Blue);
            }
            //Move captain if not in slot
            else
            {
                _lc.MovePlayer(teamACap, 0);
                _lc.SetTeam(teamACap, SlotColor.Blue);
            }

            for (int i = 0; i < teamA.Count; i++)
            {
                Slot slot = _lc.Slots[i + 2];

                if (!slot.IsUsed)
                {
                    _lc.MovePlayer(teamA[i], slot.Id);
                }
                else if (!slot.Nickname.Equals(teamA[i], StringComparison.CurrentCultureIgnoreCase))
                {
                    Move(slot.Nickname);
                    _lc.MovePlayer(teamA[i], slot.Id);
                }

                _lc.SetTeam(teamA[i], SlotColor.Blue);
            }

            int capRedSlotId = playersPerTeam + 1;
            Slot slot2 = _lc.Slots[capRedSlotId];

            //Move player away and captain to slot
            if (slot2.IsUsed && !slot1.Nickname.Equals(teamBCap, StringComparison.CurrentCultureIgnoreCase))
            {
                Move(slot2.Nickname);
                _lc.MovePlayer(teamBCap, capRedSlotId);
                _lc.SetTeam(teamBCap, SlotColor.Red);
            }
            //Move captain if not in slot
            else
            {
                _lc.MovePlayer(teamBCap, capRedSlotId);
                _lc.SetTeam(teamBCap, SlotColor.Red);
            }

            //Get wrong slots
            List<string> playersRed = teamB.ToList();
            List<int> freeSlots = new List<int>();

            for (int id = capRedSlotId + 1; id < playersPerTeam * 2; id++)
            {
                Slot slot = _lc.Slots[id];

                if (slot.IsUsed)
                    playersRed.Remove(slot.Nickname);
                else
                    freeSlots.Add(slot.Id);
            }

            for (int i = 0; i < freeSlots.Count; i++)
            {
                _lc.MovePlayer(playersRed[0], freeSlots[i]);
                _lc.SetTeam(playersRed[0], SlotColor.Red);
                playersRed.RemoveAt(0);
            }

            void Move(string player)
            {
                _lc.MovePlayer(player, nextFreeSlot);
                nextFreeSlot++;
            }
        }

        public void SendDiscordMessage(string message)
        {
            var channel = Program.DiscordHandler.GetChannelAsync(Settings.DiscordGuildId, Settings.DiscordNotifyChannelId).ConfigureAwait(false).GetAwaiter().GetResult();

            if (channel == null)
                return;

            channel.SendMessageAsync(message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public void SubmitResults()
        {
            var dchannel = Program.DiscordHandler.GetChannelAsync(Settings.DiscordGuildId, Settings.DiscordNotifyChannelId).ConfigureAwait(false).GetAwaiter().GetResult();

            var embedBuilder = new DSharpPlus.Entities.DiscordEmbedBuilder()
            {
                Title = $"Results for match {LC.Settings.RoomName} ({LC.Settings.ChannelName})",
                Description = Resources.InvisibleCharacter
            };

            embedBuilder = embedBuilder.AddField("Bans", ToString(BannedMaps))
                                       .AddField("Picks", ToString(PickedMaps))
                                       .AddField("Unplayed maps", ToString(Mappool))
                                       .AddField($"Blue vs Red : {BlueWins} vs {RedWins}", Resources.InvisibleCharacter)
                                       .AddField("Chatlog", "To be done");

            dchannel.SendMessageAsync(embed: embedBuilder.Build()).ConfigureAwait(false);
        }

        string ToString<T>(List<T> list)
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < list.Count; i++)
                sb.Append(list[i].ToString() + ", ");

            sb = sb.Remove(sb.Length - 2, 2);

            return sb.ToString();
        }

        /// <summary>
        /// Invites all players based on <see cref="Settings"/>
        /// </summary>
        public void InvitePlayers()
        {
            if (Settings.PlayersBlue != null && Settings.PlayersBlue.Count > 0)
                Invite(Settings.PlayersBlue);

            if (Settings.PlayersRed != null && Settings.PlayersRed.Count > 0)
                Invite(Settings.PlayersRed);

            _lc.Invite(Settings.CaptainBlue);
            _lc.Invite(Settings.CaptainRed);

            void Invite(List<string> players)
            {
                foreach (string p in players)
                    _lc.Invite(p);
            }
        }

        void OnMessageReceived(object sender, ChatMessage e)
        {
            for (int i = 0; i < _interactionsQueue.Count; i++)
            {
                if (!_interactionsQueue.TryDequeue(out ChatInteraction ci))
                    break;

                if (!ci.IsFromUser(e.From) ||
                    !ci.StartsWith(e.Message))
                {
                    _interactionsQueue.Enqueue(ci);
                    continue;
                }

                ci.Action?.Invoke(e.Message);
                break;
            }
        }

        void Tick(object sender, EventArgs e)
        {
            if (_tickQueue.Count == 0)
                return;

            if (_tickQueue[0].Invoke())
                _tickQueue.RemoveAt(0);
        }
    }
}
