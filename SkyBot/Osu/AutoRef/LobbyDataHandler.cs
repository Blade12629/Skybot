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
    public class LobbyDataHandler
    {
        public DateTimeOffset CreationDate { get; private set; }
        public string LobbyName { get; private set; }
        public ulong MatchId { get; private set; }

        public LobbyStatus Status { get; private set; }

        public int WinsBlue { get; private set; }
        public int WinsRed { get; private set; }

        public ulong LastMap { get; private set; }

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

            _evRunner.EnqueueEvent(EventHelper.CreateMapStartEvent());
            _slotsUsedOnMapStart = GetUsedSlots().Count;
        }

        public void OnMapFinish()
        {
            _slotsUsedOnMapStart = -1;

            Score[] currentScores = _currentScores.ToArray();
            _currentScores.Clear();

            long bluePoints = 0, redPoints = 0;

            for (int i = 0; i < currentScores.Length; i++)
            {
                if (!currentScores[i].Passed)
                    continue;

                Slot s = GetSlot(currentScores[i].Username);

                if (s == null)
                {
                    Logger.Log("Unable to find slot for player", LogLevel.Warning);
                    continue;
                }

                switch (s.Color)
                {
                    case SlotColor.Blue:
                        bluePoints += currentScores[i].UserScore;
                        break;

                    case SlotColor.Red:
                        redPoints += currentScores[i].UserScore;
                        break;
                }
            }

            _matchRounds.Add(new MatchRound(_matchRounds.Count + 1, LastMap, currentScores.ToList()));

            if (bluePoints > redPoints)
                WinsBlue++;
            else if (redPoints > bluePoints)
                WinsRed++;
            else
            {
                WinsBlue++;
                WinsRed++;
            }

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
            Slot o = GetSlot(user);
            Slot n = GetSlot(slot);

            o.Swap(n);

            _evRunner.EnqueueEvent(EventHelper.CreateUserSwitchSlotEvent(user, o, n));
        }

        public void OnUserJoinLobby(string user, int slot, SlotColor team)
        {
            Slot s = GetSlot(slot);

            s.Nickname = user;
            s.Color = team;

            _evRunner.EnqueueEvent(EventHelper.CreateJoinEvent(user, s));
        }

        public void OnUserLeaveLobby(string user)
        {
            Slot s = GetSlot(user);
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

        public Slot GetSlot(string nickname, StringComparison comparer = StringComparison.CurrentCultureIgnoreCase)
        {
            return _slots.Values.FirstOrDefault(s => s.IsUsed && s.Nickname != null && s.Nickname.Equals(nickname, comparer));
        }

        public Slot GetSlot(int slot)
        {
            if (_slots.TryGetValue(slot, out Slot s))
                return s;

            return null;
        }

        public List<Slot> GetSlots()
        {
            return _slots.Values.ToList();
        }

        public List<Slot> GetUsedSlots()
        {
            return _slots.Values.Where(s => s.IsUsed).ToList();
        }

        public List<Slot> GetUnusedSlots()
        {
            return _slots.Values.Where(s => !s.IsUsed).ToList();
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
