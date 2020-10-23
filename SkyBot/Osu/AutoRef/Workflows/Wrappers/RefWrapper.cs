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
    }
}
