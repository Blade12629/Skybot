using AutoRefTypes;
using AutoRefTypes.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefScripts
{
    public class Main : IEntryPoint
    {
        public static Main MainInstance { get; private set; }

        RefController _refController;

        public void OnLoad(ILobby lobby, IEventRunner eventRunner)
        {
            lobby.DebugLog("Loading...");

            MainInstance = this;
            _refController = new RefController(lobby, eventRunner);
        }
    }

    //Subscribe to events simply by referencing their interface
    public class RefController : EventObject, IUserJoin, IUserLeave, IUserSwitchSlot, IAllUsersReady, IMapChange, IMapStart, IMapEnd, IReceiveScore, IChatMessageReceived, ISlotUpdate, IRollReceive, IUpdate
    {
        ILobby _lobby;
        IEventRunner _eventRunner;
        bool _doUpdate = true;

        public RefController(ILobby lobby, IEventRunner er) : base(er)
        {
            _lobby = lobby;
            _eventRunner = er;
        }

        public void OnAllUsersReady()
        {
            _lobby.DebugLog("All users ready");
        }

        public void OnChatMessageReceived(IChatMessage msg)
        {
            _lobby.DebugLog($"Received message from {msg.From}: {msg.Message}");
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

        public void OnReceiveScore(IScore score)
        {
            _lobby.DebugLog($"Received score from {score.Username}, passed: {score.Passed}, score: {score.UserScore}");
        }

        public void OnRollReceive(IRoll roll)
        {
            _lobby.DebugLog($"{roll.Nickname} rolled {roll.Rolled}");
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
            if (_doUpdate)
            {
                _doUpdate = false;
                _lobby.Invite("Skyfly");

                //This will be called every tick, with the default tick rate and depending on delay it should be around 15-25 times per second
                _lobby.DebugLog("This is an update");
            }
        }
    }
}
