using SkyBot.Osu.AutoRef.Chat;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
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
        public event EventHandler<Exception> OnException;

        public LobbyController LC => _lc;

        LobbyController _lc;
        ConcurrentQueue<Action> _tickQueue;
        Queue<ChatInteraction> _interactionsQueue;

        bool _lastRollInvalid;
        Roll _lastRoll;

        long _lastPick;
        bool _lastPickInvalid;

        public AutoRefController(LobbyController lc)
        {
            _lc = lc;
            _lc.OnAfterTick += Tick;
            _lc.OnMessageReceived += OnMessageReceived;
        }

        public void AddTicks(List<Action> ticks)
        {
            ticks.ForEach(t => _tickQueue.Enqueue(t));
        }

        public void Start(string lobbyName)
        {
            _tickQueue = new ConcurrentQueue<Action>();
            _interactionsQueue = new Queue<ChatInteraction>();
            _lc.CreateLobby(lobbyName);
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
        }

        public void SortPlayers()
        {

        }

        /// <summary>
        /// Set ref specific setting
        /// </summary>
        public void SetSetting(string setting, bool value)
        {

        }

        /// <summary>
        /// Set ref specific setting
        /// </summary>
        public void SetSetting(string setting, long value)
        {

        }

        /// <summary>
        /// Set ref specific setting
        /// </summary>
        public void SetSetting(string setting, string value)
        {

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
            if (_tickQueue.TryDequeue(out Action a))
                a?.Invoke();
        }
    }
}
