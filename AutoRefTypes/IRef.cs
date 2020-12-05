using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface IRef
    {
        public IRoll LastRequestedRoll { get; }
        public IPick LastRequestedPick { get; }
        public ILobby Lobby { get; }

        /// <summary>
        /// Requests a roll from the player (osu default: min 0 and max 100)
        /// </summary>
        public void RequestRoll(string from);

        /// <summary>
        /// Requests a map pick from a player
        /// </summary>
        public void RequestPick(string from);

        /// <summary>
        /// Requests a response from the player that starts with a specific string
        /// </summary>
        public void RequestResponse(string from, string startsWith, Action<string> action);

        /// <summary>
        /// Sorts each player by their team, captains will get the first slot
        /// </summary>
        public void SortPlayers();

        /// <summary>
        /// Bans a map from the mappool
        /// </summary>
        /// <param name="map">Map id</param>
        /// <returns>Map was in mappool and has been banned</returns>
        public bool Ban(long map);

        /// <summary>
        /// Picks a map from the mappool
        /// </summary>
        /// <param name="map">Map id</param>
        /// <returns>Map was in mappool and has been picked</returns>
        public bool Pick(long map);

        /// <summary>
        /// Sends the stats to the discord channel specified in the settings
        /// </summary>
        public void SubmitResults();

        /// <summary>
        /// Same as Lobby.SendMessage
        /// </summary>
        public void SendMessage(string msg);
    }
}
