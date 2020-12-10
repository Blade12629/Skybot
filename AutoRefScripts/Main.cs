using AutoRefTypes;
using AutoRefTypes.Events;
using AutoRefTypes.Extended.Requests;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefScripts
{
    public class Main : IEntryPoint
    {
        public static Main MainInstance { get; private set; }

        RefController _refController;

        public void OnLoad(ILobby lobby, IEventRunner eventRunner, IDiscordHandler discord)
        {
            lobby.DebugLog("Loading...");

            MainInstance = this;
            _refController = new RefController(lobby, eventRunner, discord);
        }
    }

    //Subscribe to events simply by referencing their interface
    public class RefController : EventObject, IUserJoin, IUserLeave, IUserSwitchSlot, IAllUsersReady, IMapChange, 
                                              IMapStart, IMapEnd, IReceiveScore, IChatMessageReceived, ISlotUpdate, 
                                              IUpdate, IMatchStartsIn, IQueueMatchStart, IAbortMatch, IHostChange
    {
        ILobby _lobby;
        IEventRunner _eventRunner;
        IDiscordHandler _discord;

        public RefController(ILobby lobby, IEventRunner er, IDiscordHandler discord) : base(er)
        {
            _lobby = lobby;
            _eventRunner = er;
            _discord = discord;
        }

        public void OnAbortMatch()
        {
            _lobby.DebugLog("Match was aborted");
        }

        public void OnAllUsersReady()
        {
            _lobby.DebugLog("All users ready");
        }

        public void OnChatMessageReceived(IChatMessage msg)
        {
            _lobby.DebugLog($"Received message from {msg.From}: {msg.Message}");
        }

        public void OnHostChange(string newHost)
        {
            _lobby.DebugLog($"Changed host to {newHost}");
        }

        public void OnMapChange(ulong newMap)
        {
            _lobby.DebugLog("Map changed to " + newMap);
        }

        public void OnMapEnd()
        {
            _lobby.DebugLog("Map Ended");
        }

        public void OnMapStart()
        {
            _lobby.DebugLog("Map Started");
        }

        public void OnMatchStartIn(long startDelayS)
        {
            _lobby.DebugLog($"Match starts in {startDelayS} seconds");
        }

        public void OnQueueMatchStart(long startDelayS)
        {
            _lobby.DebugLog($"Match enqueued to start in {startDelayS} seconds");
        }

        public void OnReceiveScore(IScore score)
        {
            _lobby.DebugLog($"Received score from {score.Username}, passed: {score.Passed}, score: {score.UserScore}");
        }

        public void OnSlotUpdate(ISlot slot)
        {
            _lobby.DebugLog($"Slot update for slot " + slot.Id);
        }

        public void OnUserJoin(string nick, ISlot slot)
        {
            _lobby.DebugLog(nick + " joined the lobby");
        }

        public void OnUserLeave(string nick)
        {
            _lobby.DebugLog(nick + " left the lobby");
        }

        public void OnUserSwitchSlot(string nick, ISlot oldSlot, ISlot newSlot)
        {
            _lobby.DebugLog($"User {nick} switched from slot {oldSlot.Id} to slot {newSlot.Id}");
        }

        public void Update()
        {

        }
    }
}
