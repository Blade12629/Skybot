using AutoRefTypes;
using SkyBot.Osu.AutoRef.Data;
using SkyBot.Osu.AutoRef.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class LobbyDataHandler : ILobbyDataHandler
    {
        public DateTimeOffset CreationDate { get; private set; }
        public string LobbyName { get; private set; }
        public ulong MatchId { get; private set; }

        public LobbyStatus Status { get; private set; }

        public ulong LastMap { get; private set; }

        public IReadOnlyList<IMatchRound> MatchRounds { get => _matchRounds; }
        public IReadOnlyList<IScore> Scores { get => _currentScores; }

        List<IMatchRound> _matchRounds;
        List<Score> _currentScores;

        Dictionary<int, Slot> _slots;
        EventRunner _evRunner;
        int _slotsUsedOnMapStart;

        public LobbyDataHandler(EventRunner evr)
        {
            _matchRounds = new List<IMatchRound>();
            _evRunner = evr;
            _slots = new Dictionary<int, Slot>();
            for (int i = 0; i < 16; i++)
                _slots.Add(i + 1, new Slot(i + 1));

            _currentScores = new List<Score>();
        }


        public void OnCreation(string lobbyName)
        {
            Status = LobbyStatus.Creating;
            LobbyName = lobbyName;
        }

        public void OnCreated(ulong matchId)
        {
            Status = LobbyStatus.Created;
            CreationDate = new DateTimeOffset(DateTime.UtcNow, TimeSpan.Zero);
            MatchId = matchId;
        }

        public void OnMapStart()
        {
            Status = LobbyStatus.Playing;
            _currentScores.Clear();

            foreach (Slot slot in _slots.Values)
                slot.IsReady = false;

            _evRunner.EnqueueEvent(EventHelper.CreateMapStartEvent());
            _slotsUsedOnMapStart = GetUsedSlots().Count;
        }

        public void OnMapFinish()
        {
            _slotsUsedOnMapStart = -1;

            IReadOnlyList<IScore> scores = _currentScores.Select(s => (IScore)s).ToList();
            _matchRounds.Add(new MatchRound(_matchRounds.Count + 1, LastMap, scores));

            Status = LobbyStatus.Created;
            _evRunner.EnqueueEvent(EventHelper.CreateMapEndEvent());
        }

        public void OnMapChange(ulong newMap)
        {
            LastMap = newMap;
            _evRunner.EnqueueEvent(EventHelper.CreateMapChangeEvent(newMap));
        }

        public void OnUserSwitchSlot(string user, int slot)
        {
            Slot o = (Slot)GetSlot(user);
            Slot n = (Slot)GetSlot(slot);

            o.Swap(n);

            _evRunner.EnqueueEvent(EventHelper.CreateUserSwitchSlotEvent(user, o, n));
        }

        public void OnUserJoinLobby(string user, int slot, SlotColor team)
        {
            Slot s = (Slot)GetSlot(slot);

            s.Nickname = user;
            s.Color = team;

            _evRunner.EnqueueEvent(EventHelper.CreateJoinEvent(user, s));
        }

        public void OnUserLeaveLobby(string user)
        {
            Slot s = (Slot)GetSlot(user);
            s.Reset();

            _evRunner.EnqueueEvent(EventHelper.CreateLeaveEvent(user));
        }

        public void OnReceiveScore(string username, long score, bool passed)
        {
            lock(((ICollection)_currentScores).SyncRoot)
            {
                Score s = new Score(username, score, passed);
                _currentScores.Add(s);

                _evRunner.EnqueueEvent(EventHelper.CreateReceiveScoreEvent(s));

                if (_currentScores.Count == _slotsUsedOnMapStart)
                    OnMapFinish();
            }
        }

        public void OnChangeHost(string newHost)
        {
            _evRunner.EnqueueEvent(EventHelper.CreateHostChangeEvent(newHost));
        }

        public void OnAllPlayersReady()
        {
            IEnumerable<Slot> slots = _slots.Values.Where(s => s.IsUsed);

            foreach (var slot in slots)
                slot.IsReady = true;

            _evRunner.EnqueueEvent(EventHelper.CreateAllUsersReadyEvent());
        }

        public void OnSlotUpdate(Slot slot)
        {
            _evRunner.EnqueueEvent(EventHelper.CreateSlotUpdateEvent(slot));
        }

        public void OnMatchStartsIn(long startDelayS)
        {
            _evRunner.EnqueueEvent(EventHelper.CreateMatchStartInEvent(startDelayS));
        }

        public void OnQueueMatchStart(long startDelayS)
        {
            _evRunner.EnqueueEvent(EventHelper.CreateQueueMatchStartEvent(startDelayS));
        }

        public void OnAbortMatch()
        {
            _evRunner.EnqueueEvent(EventHelper.CreateAbortMatchEvent());
        }

        public ISlot GetSlot(string nickname, StringComparison comparer = StringComparison.CurrentCultureIgnoreCase)
        {
            return _slots.Values.FirstOrDefault(s => s.IsUsed && s.Nickname != null && s.Nickname.Equals(nickname, comparer));
        }

        public ISlot GetSlot(int slot)
        {
            if (_slots.TryGetValue(slot, out Slot s))
                return s;

            return null;
        }

        public ISlot GetFirstUnusedSlot()
        {
            return _slots.Values.FirstOrDefault(s => !s.IsUsed) as ISlot;
        }

        public ISlot GetFirstUsedSlot()
        {
            return _slots.Values.FirstOrDefault(s => s.IsUsed) as ISlot;
        }

        public ISlot GetSlot(Func<ISlot, bool> predicate)
        {
            return _slots.Values.FirstOrDefault(s => predicate(s));
        }

        public List<ISlot> GetSlots(Func<ISlot, bool> predicate)
        {
            return _slots.Values.Where(s => predicate(s))
                                .Select(s => (ISlot)s)
                                .ToList();
        }

        public List<ISlot> GetSlots()
        {
            return _slots.Values.Select(s => (ISlot)s)
                                .ToList();
        }

        public List<ISlot> GetUsedSlots()
        {
            return _slots.Values.Where(s => s.IsUsed)
                                .Select(s => (ISlot)s)
                                .ToList();
        }

        public List<ISlot> GetUnusedSlots()
        {
            return _slots.Values.Where(s => !s.IsUsed)
                                .Select(s => (ISlot)s)
                                .ToList();
        }
    }

    public enum LobbyStatus
    {
        None,
        Creating,
        Created,
        Playing,
        Closed
    }
}
