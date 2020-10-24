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
            return _arc.LastRollValid;
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
            return _arc.LastPickValid;
        }
    }
}
