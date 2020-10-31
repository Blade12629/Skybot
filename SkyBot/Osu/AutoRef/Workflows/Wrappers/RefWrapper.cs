using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflows.Wrappers
{
    public class RefWrapper
    {
        AutoRefController _arc;

        internal RefWrapper(AutoRefController arc)
        {
            _arc = arc;
        }

        /// <summary>
        /// Requests a roll from the player (osu default: min 0 and max 100)
        /// </summary>
        public void RequestRoll(string from)
        {
            _arc.RequestRoll(from);
        }

        /// <summary>
        /// Requests rolls from captain blue and red
        /// </summary>
        public void RequestRolls()
        {
            _arc.RequestRolls();
        }

        /// <summary>
        /// Requests a map pick from a player
        /// </summary>
        public void RequestPick(string from)
        {
            _arc.RequestPick(from);
        }

        /// <summary>
        /// Requests a response from the player that starts with a specific string
        /// </summary>
        public void RequestResponse(string from, string startsWith, Action<string> action)
        {
            _arc.RequestResponse(from, startsWith, action);
        }
    
        /// <summary>
        /// Sorts each player by their team, captains will get the first slot
        /// </summary>
        public void SortPlayers()
        {
            _arc.SortPlayers();
        }

        /// <summary>
        /// Gets the last roll
        /// </summary>
        public RollWrapper GetLastRoll()
        {
            return _arc.LastRoll;
        }

        /// <summary>
        /// Returns if the last roll is a valid roll
        /// </summary>
        public bool IsLastRollValid()
        {
            return _arc.LastRollValid && _arc.LastRoll != null;
        }

        /// <summary>
        /// Gets the last pick
        /// </summary>
        public long GetLastPick()
        {
            return _arc.LastPick;
        }

        /// <summary>
        /// Returns if the last pick is a valid pick
        /// </summary>
        public bool IsLastPickValid()
        {
            return _arc.LastPickValid && _arc.LastPick > 0;
        }

        /// <summary>
        /// Invites all players
        /// </summary>
        public void InviteAllPlayers()
        {
            _arc.InvitePlayers();
        }

        public void DebugLog(string message)
        {
            Logger.Log(message);
        }

        /// <summary>
        /// Did we pass a specific delay since the match creation
        /// </summary>
        /// <param name="duration">Delay in seconds</param>
        /// <returns>Time passed</returns>
        public bool PassedDelaySinceStart(int duration)
        {
            if (_arc.LC.IsClosed || _arc.LC.CreationDate == DateTime.UtcNow ||
                duration <= 0)
                return false;

            TimeSpan passed = DateTime.UtcNow - _arc.LC.CreationDate;

            return passed.TotalSeconds <= duration;
        }

        /// <summary>
        /// Requests the lobby to be closed
        /// </summary>
        public void CloseLobby()
        {
            _arc.LC.EnqueueCloseLobby();
        }

        /// <summary>
        /// Bans a map from the mappool
        /// </summary>
        /// <param name="map">Map id</param>
        /// <returns>Map was in mappool and has been banned</returns>
        public bool Ban(long map)
        {
            if (!_arc.Mappool.Contains(map))
                return false;

            _arc.BannedMaps.Add(map);
            _arc.Mappool.Remove(map);

            return true;
        }

        /// <summary>
        /// Picks a map from the mappool
        /// </summary>
        /// <param name="map">Map id</param>
        /// <returns>Map was in mappool and has been picked</returns>
        public bool Pick(long map)
        {
            if (!_arc.Mappool.Contains(map))
                return false;

            _arc.PickedMaps.Add(map);
            _arc.Mappool.Remove(map);

            return true;
        }

        /// <summary>
        /// Swaps the current captain with the other captain (Blue -> Red, Red -> Blue)
        /// </summary>
        public void SetNextCaptainPick()
        {
            _arc.CurrentCaptain = _arc.CurrentCaptain == SlotColor.Blue ? SlotColor.Red : SlotColor.Blue;
        }

        /// <summary>
        /// Check if the map has finished
        /// </summary>
        /// <returns>Map finished</returns>
        public bool MapFinished()
        {
            return _arc.LC.LatestScores.Count >= 2;
        }

        /// <summary>
        /// Starts the map with a default countdown of 10 seconds
        /// </summary>
        public void Play()
        {
            Play(10);
        }

        /// <summary>
        /// Starts the map with a countdown
        /// </summary>
        /// <param name="seconds">Delay in seconds to start</param>
        public void Play(int seconds)
        {
            _arc.LC.StartMap(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Sets the current workflow state
        /// </summary>
        /// <param name="state">New workflow state</param>
        public void SetState(int state)
        {
            _arc.WorkflowState = state;
        }

        /// <summary>
        /// Gets the current workflow state
        /// </summary>
        /// <returns>Workflow state</returns>
        public int GetState()
        {
            return _arc.WorkflowState;
        }

        /// <summary>
        /// Sends the stats to the discord channel specified in the settings
        /// </summary>
        public void SubmitResults()
        {
            _arc.SubmitResults();
        }

        public void SendMessage(string msg)
        {
            Logger.Log(msg);
            _arc.SendMessage(msg);
        }
    }
}
