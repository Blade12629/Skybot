using AutoRefTypes;
using SkyBot.Osu.AutoRef.Chat;
using SkyBot.Osu.AutoRef.Data;
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
    public partial class AutoRefController : IRef
    {
        public LobbyController LC => _lc;
        public AutoRefSettings Settings { get; set; }

        public IRoll LastRequestedRoll { get; private set; }
        public IPick LastRequestedPick { get; private set; }
        public ILobby Lobby { get => _lc; }

        public List<long> BannedMaps { get; set; }
        public List<long> PickedMaps { get; set; }
        public List<long> Mappool { get; set; }
        public long TieBreaker { get; set; }

        LobbyController _lc;
        List<Func<bool>> _tickQueue;
        Queue<ChatInteraction> _interactionsQueue;

        public AutoRefController(LobbyController lc)
        {
            _tickQueue = new List<Func<bool>>();
            _lc = lc;
            _lc.OnMessageReceived += OnMessageReceived;
        }

        public void AddTicks(List<Func<bool>> ticks)
        {
            _tickQueue.AddRange(ticks);
        }

        public void Start(string lobbyName)
        {
            _interactionsQueue = new Queue<ChatInteraction>();
            _lc.CreateLobby(lobbyName);
        }

        public void TestRun()
        {
            for (int i = 0; i < _tickQueue.Count; i++)
                _tickQueue[i].Invoke();
        }

        public void RequestResponse(string from, string startsWith, Action<string> action)
        {
            if (string.IsNullOrEmpty(from))
                throw new ArgumentNullException(nameof(from));
            else if (string.IsNullOrEmpty(startsWith))
                throw new ArgumentNullException(nameof(startsWith));
            else if (action == null)
                throw new ArgumentNullException(nameof(action));

            _interactionsQueue.Enqueue(new ChatInteraction(from, startsWith, action));
        }

        /// <summary>
        /// Requests a roll via !roll
        /// </summary>
        /// <param name="from">User to request roll from</param>
        public void RequestRoll(string from)
        {
            LastRequestedRoll = null;
            string nick = from;
            long min = 0;
            long max = 100;

            RequestResponse(from, "!roll ", new Action<string>(s =>
            {
                string[] split = s.Split(' ');

                if (split != null && split.Length > 1)
                {
                    if (long.TryParse(split[1], out long min_))
                    {
                        min = min_;

                        //!roll min max
                        if (split.Length >= 3 && long.TryParse(split[2], out long max_))
                            max = max_;
                    }
                }
            }));

            RequestResponse("banchobot", $"{from} rolls", new Action<string>(s =>
            {
                string[] split = s.Remove(0, from.Length + 1).Split(' ');
                long rolled = long.Parse(split[1]);

                LastRequestedRoll = new Roll(nick, min, max, rolled);
            }));

            SendMessage($"{from} please roll via !roll");
        }

        public void SendMessage(string message)
        {
            _lc.SendMessage($"——— {message} ———");
        }

        /// <summary>
        /// Requests a pick via !pick mapId
        /// </summary>
        /// <param name="from">User to request pick from</param>
        public void RequestPick(string from)
        {
            LastRequestedPick = null;

            RequestResponse(from, "!pick ", new Action<string>(s =>
            {
                string[] split = s.Remove(0, from.Length + 1).Split(' ');

                if (split == null || split.Length < 2)
                {
                    LastRequestedPick = new Pick(from, 0);
                    return;
                }

                if (!ulong.TryParse(split[1], out ulong rolled))
                {
                    LastRequestedPick = new Pick(from, 0);
                    return;
                }

                LastRequestedPick = new Pick(from, rolled);
            }));

            SendMessage($"{from} please pick a map via !pick mapId");
        }

        /// <summary>
        /// Sorts players, this requires a max of 10 players and no players above slot 10
        /// </summary>
        /// <param name="players"></param>
        /// <param name="slotStart"></param>
        public void SortTeams(List<string> teamA, string teamACap, List<string> teamB, string teamBCap, int playersPerTeam)
        {
            int nextFreeSlot = 11;

            ISlot slot1 = _lc.Slots[1];

            //Move player away and captain to slot
            if (slot1.IsUsed && !slot1.Nickname.Equals(teamACap, StringComparison.CurrentCultureIgnoreCase))
            {
                Move(slot1.Nickname);
                _lc.SetSlot(teamACap, 0);
                _lc.SetTeam(teamACap, SlotColor.Blue);
            }
            //Move captain if not in slot
            else
            {
                _lc.SetSlot(teamACap, 0);
                _lc.SetTeam(teamACap, SlotColor.Blue);
            }

            for (int i = 0; i < teamA.Count; i++)
            {
                ISlot slot = _lc.Slots[i + 2];

                if (!slot.IsUsed)
                {
                    _lc.SetSlot(teamA[i], slot.Id);
                }
                else if (!slot.Nickname.Equals(teamA[i], StringComparison.CurrentCultureIgnoreCase))
                {
                    Move(slot.Nickname);
                    _lc.SetSlot(teamA[i], slot.Id);
                }

                _lc.SetTeam(teamA[i], SlotColor.Blue);
            }

            int capRedSlotId = playersPerTeam + 1;
            ISlot slot2 = _lc.Slots[capRedSlotId];

            //Move player away and captain to slot
            if (slot2.IsUsed && !slot1.Nickname.Equals(teamBCap, StringComparison.CurrentCultureIgnoreCase))
            {
                Move(slot2.Nickname);
                _lc.SetSlot(teamBCap, capRedSlotId);
                _lc.SetTeam(teamBCap, SlotColor.Red);
            }
            //Move captain if not in slot
            else
            {
                _lc.SetSlot(teamBCap, capRedSlotId);
                _lc.SetTeam(teamBCap, SlotColor.Red);
            }

            //Get wrong slots
            List<string> playersRed = teamB.ToList();
            List<int> freeSlots = new List<int>();

            for (int id = capRedSlotId + 1; id < playersPerTeam * 2; id++)
            {
                ISlot slot = _lc.Slots[id];

                if (slot.IsUsed)
                    playersRed.Remove(slot.Nickname);
                else
                    freeSlots.Add(slot.Id);
            }

            for (int i = 0; i < freeSlots.Count; i++)
            {
                _lc.SetSlot(playersRed[0], freeSlots[i]);
                _lc.SetTeam(playersRed[0], SlotColor.Red);
                playersRed.RemoveAt(0);
            }

            void Move(string player)
            {
                _lc.SetSlot(player, nextFreeSlot);
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
                                       .AddField($"Blue vs Red : {Lobby.BlueWins} vs {Lobby.RedWins}", Resources.InvisibleCharacter)
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

        public void OnTick()
        {
            if (_tickQueue.Count == 0)
                return;

            for (int i = 0; i < _tickQueue.Count; i++)
            {
                try
                {
                    if (_tickQueue[i].Invoke())
                    {
                        _tickQueue.RemoveAt(i);
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex, LogLevel.Error);
                    throw ex;
                }
            }
        }
    }

    public partial class AutoRefController
    {

        /// <summary>
        /// Sorts the players based on <see cref="Settings"/>
        /// </summary>
        public void SortPlayers()
        {
            SortTeams(Settings.PlayersBlue, Settings.CaptainBlue, Settings.PlayersRed, Settings.CaptainRed, Settings.PlayersPerTeam);
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

            if (!string.IsNullOrEmpty(Settings.CaptainBlue))
                _lc.Invite(Settings.CaptainBlue);

            if (!string.IsNullOrEmpty(Settings.CaptainRed))
                _lc.Invite(Settings.CaptainRed);

            void Invite(List<string> players)
            {
                foreach (string p in players)
                    _lc.Invite(p);
            }
        }

        public bool Ban(long map)
        {
            if (map <= 0)
                throw new ArgumentOutOfRangeException(nameof(map));
            else if (BannedMaps.Contains(map))
                return false;

            BannedMaps.Add(map);
            return true;
        }

        public bool Pick(long map)
        {
            if (map <= 0)
                throw new ArgumentOutOfRangeException(nameof(map));
            else if (PickedMaps.Contains(map))
                return false;

            PickedMaps.Add(map);
            return true;
        }
    }
}
