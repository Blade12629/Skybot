using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface ILobby
    {
        /// <summary>
        /// Invites a player
        /// </summary>
        public void Invite(string player);

        /// <summary>
        /// Locks the lobby
        /// </summary>
        public void Lock();

        /// <summary>
        /// Unlocks the lobby
        /// </summary>
        public void Unlock();

        /// <summary>
        /// Converts a nickname to an irc nickname
        /// </summary>
        /// <returns>Example: User 1 -> User_1</returns>
        public string ToIrcNick(string user);

        /// <summary>
        /// Converts an irc nickname to a nickname
        /// </summary>
        /// <returns>Example: User_1 -> User 1</returns>
        public string FromIrcNick(string user);

        /// <summary>
        /// Sets the lobby team mode and win condition
        /// </summary>
        public void SetLobby(TeamMode teamMode, WinCondition? winCondition, int? slots);

        /// <summary>
        /// Moves a player to another slot
        /// </summary>
        /// <param name="player"></param>
        /// <param name="slot"></param>
        public void SetSlot(string player, int slot);

        /// <summary>
        /// Sets the host
        /// </summary>
        /// <param name="host"></param>
        public void SetHost(string host = null);

        /// <summary>
        /// Requests settings to be sent from bancho
        /// </summary>
        public void RequestSettings();

        /// <summary>
        /// Starts the map after <paramref name="delay"/>
        /// </summary>
        /// <param name="delay">Start delay, <see cref="TimeSpan.Zero"/> for default delay of 10 seconds</param>
        public void StartMap(TimeSpan delay);

        /// <summary>
        /// Aborts the current map
        /// </summary>
        public void AbortMap();

        /// <summary>
        /// Changes a players team
        /// </summary>
        public void SetTeam(string player, SlotColor color);

        /// <summary>
        /// Sets a specific map
        /// </summary>
        /// <param name="mode">0: osu!, 1: Taiko, 2: Catch The Beat, 3: osu!Mania</param>
        public void SetMap(long map, int? mode = null);

        /// <summary>
        /// Starts a timer
        /// </summary>
        public void StartTimer(TimeSpan delay);

        /// <summary>
        /// Aborts the currently running timer
        /// </summary>
        public void AbortTimer();

        /// <summary>
        /// Kicks a player
        /// </summary>
        /// <param name="player"></param>
        public void Kick(string player);

        /// <summary>
        /// Adds a referee
        /// </summary>
        /// <param name="ref"></param>
        public void AddRef(string @ref);

        /// <summary>
        /// Adds multiple referees
        /// </summary>
        /// <param name="refs"></param>
        public void AddRefs(params string[] refs);

        /// <summary>
        /// Removes a referee
        /// </summary>
        /// <param name="ref"></param>
        public void RemoveRef(string @ref);

        /// <summary>
        /// Removes multiple referees
        /// </summary>
        /// <param name="refs"></param>
        public void RemoveRefs(params string[] refs);

        /// <summary>
        /// Lists all refs
        /// </summary>
        public void ListRefs();

        /// <summary>
        /// Changes current mods
        /// </summary>
        public void SetMods(string mods = null, bool freemod = false);

        /// <summary>
        /// Set current mods to nomod
        /// </summary>
        public void SetNomod();

        /// <summary>
        /// Clears all mods and enables freemod
        /// </summary>
        public void SetFreemod();

        /// <summary>
        /// Sends a message that will be formatted to "——— <paramref name="message"/> ———"
        /// <para>This is the default format used for normal messages to make them clearly distinguishable
        /// from any message that is not directed towards the players</para>
        /// </summary>
        public void SendChannelMessage(string message);

        public void DebugLog(string msg);
    }
}
