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
    public class RefController : EventObject, IUserJoin, IUserLeave, IRollReceive, IMapChange, IUpdate
    {
        ILobby _lobby;
        IEventRunner _eventRunner;
        bool _doUpdate = true;

        public RefController(ILobby lobby, IEventRunner er) : base(er)
        {
            _lobby = lobby;
            _eventRunner = er;

            //In order to enable any event object you have to register it
            //unregistered gameobjects are simply ignored
            //Do not call eventRunner.Register();, instead always use eventObject.Register(); like below
            Register(er);

            //Use eventRunner.Delete(eventObject) to deregister an object
        }

        public void OnMapChange(long newMap)
        {
            _lobby.DebugLog("Map changed to " + newMap);
        }

        public void OnRollReceive(IRoll roll)
        {
            _lobby.DebugLog($"{roll.Nickname} rolled {roll.Rolled}");
        }

        public void OnUserJoin(string nick, ISlot slot)
        {
            _lobby.DebugLog(nick + " joined the lobby");
        }

        public void OnUserLeave(string nick)
        {
            _lobby.DebugLog(nick + " left the lobby");
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
