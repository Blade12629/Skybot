using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes.Events
{
    /// <summary>
    /// Called each tick
    /// </summary>
    public interface IUpdate : IEvent
    {
        public void Update();
    }


    /// <summary>
    /// Called when a user joins
    /// </summary>
    public interface IUserJoin : IEvent
    {
        public void OnUserJoin(string nick, ISlot slot);
    }

    /// <summary>
    /// Called when a user leaves
    /// </summary>
    public interface IUserLeave : IEvent
    {
        public void OnUserLeave(string nick);
    }

    /// <summary>
    /// Called when a user switches his slot or gets moved
    /// </summary>
    public interface IUserSwitchSlot : IEvent
    {
        public void OnUserSwitchSlot(string nick, ISlot oldSlot, ISlot newSlot);
    }

    /// <summary>
    /// Called when all users are ready
    /// </summary>
    public interface IAllUsersReady : IEvent
    {
        public void OnAllUsersReady();
    }

    /// <summary>
    /// Called when the map changes
    /// </summary>
    public interface IMapChange : IEvent
    {
        public void OnMapChange(ulong newMap);
    }

    /// <summary>
    /// Called when the map starts
    /// </summary>
    public interface IMapStart : IEvent
    {
        public void OnMapStart();
    }

    /// <summary>
    /// Called when the map ends
    /// </summary>
    public interface IMapEnd : IEvent
    {
        public void OnMapEnd();
    }


    /// <summary>
    /// Called when the last requested roll is received
    /// </summary>
    public interface IReceiveScore : IEvent
    {
        public void OnReceiveScore(IScore score);
    }

    public interface IChatMessageReceived : IEvent
    {
        public void OnChatMessageReceived(IChatMessage msg);
    }

    public interface ISlotUpdate : IEvent
    {
        public void OnSlotUpdate(ISlot slot);
    }

    public interface IRollReceive : IEvent
    {
        public void OnRollReceive(IRoll roll);
    }
}
