using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflows.Wrappers
{
    /// <summary>
    /// Used to access the osu! mp lobby
    /// </summary>
    public class LobbyWrapper
    {
        LobbyController _lc;

        internal LobbyWrapper(LobbyController lc)
        {
            _lc = lc;
        }

        /// <summary>
        /// Signals the <see cref="LobbyController"/> to close the lobby
        /// </summary>
        public void Close()
        {
            _lc.EnqueueCloseLobby();
        }

        /// <summary>
        /// Invites a player
        /// </summary>
        public void Invite(string player)
        {
            _lc.Invite(player);
        }

        /// <summary>
        /// Locks the lobby
        /// </summary>
        public void Lock()
        {
            _lc.LockMatch();
        }

        /// <summary>
        /// Unlocks the lobby
        /// </summary>
        public void Unlock()
        {
            _lc.UnlockMatch();
        }

        /// <summary>
        /// Converts a nickname to an irc nickname
        /// </summary>
        /// <returns>Example: User 1 -> User_1</returns>
#pragma warning disable CA1822 // Mark members as static
        public string ToIrcNick(string user)
#pragma warning restore CA1822 // Mark members as static
        {
            return LobbyController.ToIrcNick(user);
        }

        /// <summary>
        /// Converts an irc nickname to a nickname
        /// </summary>
        /// <returns>Example: User_1 -> User 1</returns>
#pragma warning disable CA1822 // Mark members as static
        public string FromIrcNick(string user)
#pragma warning restore CA1822 // Mark members as static
        {
            return LobbyController.FromIrcNick(user);
        }

        /// <summary>
        /// Sets the lobby team mode and win condition
        /// </summary>
        /// <param name="teamMode"><see cref="TeamMode"/></param>
        /// <param name="winCondition"><see cref="WinCondition"/></param>
        public void SetLobby(int teamMode, int winCondition)
        {
            SetLobby((TeamMode)teamMode, (WinCondition)winCondition);
        }

        /// <summary>
        /// Sets the lobby team mode and win condition
        /// </summary>
        public void SetLobby(TeamMode teamMode, WinCondition winCondition)
        {
            _lc.SetLobby(teamMode, winCondition, 16);
        }

        /// <summary>
        /// Moves a player to another slot
        /// </summary>
        /// <param name="player"></param>
        /// <param name="slot"></param>
        public void Move(string player, int slot)
        {
            _lc.MovePlayer(player, slot);
        }

        /// <summary>
        /// Sets the host
        /// </summary>
        /// <param name="host"></param>
        public void Host(string host)
        {
            _lc.SetHost(host);
        }

        /// <summary>
        /// Clears the host, equivalent to <see cref="ClearHost"/>
        /// </summary>
        public void Host()
        {
            ClearHost();
        }

        /// <summary>
        /// Clears the host
        /// </summary>
        public void ClearHost()
        {
            _lc.SetHost();
        }

        /// <summary>
        /// Requests settings to be sent from bancho
        /// </summary>
        public void Settings()
        {
            _lc.RefreshSettings();
        }

        /// <summary>
        /// Starts the map instantly
        /// </summary>
        public void StartMap()
        {
            StartMap(0);
        }

        /// <summary>
        /// Starts the map after an delay
        /// </summary>
        public void StartMap(int seconds)
        {
            StartMap(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Starts the map after an delay
        /// </summary>
        public void StartMap(TimeSpan delay)
        {
            _lc.StartMap(delay);
        }

        /// <summary>
        /// Aborts the current map
        /// </summary>
        public void AbortMap()
        {
            _lc.AbortMap();
        }

        /// <summary>
        /// Changes a players team
        /// </summary>
        /// <param name="color"><see cref="SlotColor"/></param>
        public void SetTeam(string player, int color)
        {
            SetTeam(player, (SlotColor)color);
        }

        /// <summary>
        /// Changes a players team
        /// </summary>
        public void SetTeam(string player, SlotColor color)
        {
            _lc.SetTeam(player, color);
        }

        /// <summary>
        /// Sets a specific map
        /// </summary>
        /// <param name="mode">0: osu!, 1: Taiko, 2: Catch The Beat, 3: osu!Mania</param>
        public void SetMap(long map, int mode)
        {
            _lc.SetMap(map, mode);
        }

        /// <summary>
        /// Sets a specific map
        /// </summary>
        public void SetMap(long map)
        {
            _lc.SetMap(map);
        }

        /// <summary>
        /// Starts a timer with the default duration of 30 seconds
        /// </summary>
        public void StartTimer()
        {
            StartTimer(30);
        }

        /// <summary>
        /// Starts a timer
        /// </summary>
        public void StartTimer(int seconds)
        {
            StartTimer(TimeSpan.FromSeconds(seconds));
        }

        /// <summary>
        /// Starts a timer
        /// </summary>
        public void StartTimer(TimeSpan delay)
        {
            _lc.StartTimer(delay);
        }

        /// <summary>
        /// Aborts the currently running timer
        /// </summary>
        public void AbortTimer()
        {
            _lc.AbortTimer();
        }

        /// <summary>
        /// Kicks a player
        /// </summary>
        /// <param name="player"></param>
        public void Kick(string player)
        {
            _lc.Kick(player);
        }

        /// <summary>
        /// Adds a referee
        /// </summary>
        /// <param name="ref"></param>
        public void AddRef(string @ref)
        {
            AddRefs(@ref);
        }

        /// <summary>
        /// Adds multiple referees
        /// </summary>
        /// <param name="refs"></param>
        public void AddRefs(params string[] refs)
        {
            _lc.AddRefs(refs);
        }

        /// <summary>
        /// Removes a referee
        /// </summary>
        /// <param name="ref"></param>
        public void RemoveRef(string @ref)
        {
            RemoveRefs(@ref);
        }

        /// <summary>
        /// Removes multiple referees
        /// </summary>
        /// <param name="refs"></param>
        public void RemoveRefs(params string[] refs)
        {
            _lc.AddRefs(refs);
        }

        /// <summary>
        /// Lists all refs
        /// </summary>
        public void ListRefs()
        {
            _lc.ListRefs();
        }

        /// <summary>
        /// Changes current mods
        /// </summary>
        public void SetMods(string mods, bool freemod)
        {
            _lc.SetMods(mods, freemod);
        }

        /// <summary>
        /// Changes current mods
        /// </summary>
        public void SetMods(bool freemod, params string[] mods)
        {
            if (mods == null ||
                mods.Length == 0)
                return;

            StringBuilder builder = new StringBuilder();

            for (int i = 0; i < mods.Length; i++)
                builder.Append($"{mods[i]} ");

            builder.Remove(builder.Length - 1, 1);

            SetMods(builder.ToString(), freemod);
        }

        /// <summary>
        /// Changes current mods
        /// </summary>
        public void SetMods(params string[] mods)
        {
            SetMods(false, mods);
        }

        /// <summary>
        /// Changes current mods
        /// </summary>
        public void SetMods(string mods)
        {
            SetMods(mods, false);
        }

        /// <summary>
        /// Set current mods to nomod
        /// </summary>
        public void SetNomod()
        {
            _lc.SetNomod();
        }

        /// <summary>
        /// Clears all mods and enables freemod
        /// </summary>
        public void SetFreemod()
        {
            _lc.SetFreemod();
        }



        /// <summary>
        /// Gets all latest scores
        /// </summary>
        /// <returns></returns>
        public ScoreWrapper[] GetLatestScores()
        {
            return _lc.LatestScores.Select(s => (ScoreWrapper)s).ToArray();
        }

        /// <summary>
        /// Clears all latest scores
        /// </summary>
        public void ClearLatestScores()
        {
            _lc.LatestScores.Clear();
        }

        /// <summary>
        /// Gets all scores
        /// </summary>
        /// <returns></returns>
        public ScoreWrapper[] GetAllScores()
        {
            return _lc.Scores.Select(s => (ScoreWrapper)s).ToArray();
        }

        /// <summary>
        /// Gets the count of the latest scores
        /// </summary>
        /// <returns></returns>
        public int GetLatestScoresCount()
        {
            return _lc.LatestScores.Count;
        }

        /// <summary>
        /// Gets the count of all scores
        /// </summary>
        /// <returns></returns>
        public int GetTotalScoresCount()
        {
            return _lc.Scores.Count;
        }

        /// <summary>
        /// Sends a message that will be formatted to "——— <paramref name="message"/> ———"
        /// <para>This is the default format used for normal messages to make them clearly distinguishable
        /// from any message that is not directed towards the players</para>
        /// </summary>
        public void SendMessage(string message)
        {
            SendMessage(message, false);
        }

        /// <summary>
        /// Sends a message that will be formatted to "——— <paramref name="message"/> ———" if <paramref name="raw"/> is <see cref="false"/>
        /// <para><paramref name="raw"/> == <see cref="false"/>: This is the default format used for normal messages to make them clearly distinguishable
        /// from any message that is not directed towards the players</para>
        /// </summary>
        public void SendMessage(string message, bool raw)
        {
            if (raw)
                _lc.SendMessage(message);
            else
                _lc.SendMessage($"——— {message} ———");
        }

        /// <summary>
        /// Gets the amount of players currently in the lobby
        /// </summary>
        public int GetTotalPlayers()
        {
            return _lc.Slots.Count(s => s.Value.IsUsed);
        }

        /// <summary>
        /// Gets a slot
        /// </summary>
        /// <param name="slot">slot id, starting at 1</param>
        public SlotWrapper GetSlot(int slot)
        {
            return _lc.Slots[slot];
        }

        /// <summary>
        /// Gets a slot
        /// </summary>
        public SlotWrapper GetSlot(string user)
        {
            return _lc.GetSlot(user);
        }
    }
}
