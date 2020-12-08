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
        /// <summary>
        /// Called each tick
        /// </summary>
        public void Update();
    }


    /// <summary>
    /// Called when a user joins
    /// </summary>
    public interface IUserJoin : IEvent
    {
        /// <summary>
        /// Called when a user joins
        /// </summary>
        public void OnUserJoin(string nick, ISlot slot);
    }

    /// <summary>
    /// Called when a user leaves
    /// </summary>
    public interface IUserLeave : IEvent
    {
        /// <summary>
        /// Called when a user leaves
        /// </summary>
        public void OnUserLeave(string nick);
    }

    /// <summary>
    /// Called when a user switches his slot or gets moved
    /// </summary>
    public interface IUserSwitchSlot : IEvent
    {
        /// <summary>
        /// Called when a user switches his slot or gets moved
        /// </summary>
        public void OnUserSwitchSlot(string nick, ISlot oldSlot, ISlot newSlot);
    }

    /// <summary>
    /// Called when all users are ready
    /// </summary>
    public interface IAllUsersReady : IEvent
    {
        /// <summary>
        /// Called when all users are ready
        /// </summary>
        public void OnAllUsersReady();
    }

    /// <summary>
    /// Called when the map changes
    /// </summary>
    public interface IMapChange : IEvent
    {
        /// <summary>
        /// Called when the map changes
        /// </summary>
        public void OnMapChange(ulong newMap);
    }

    /// <summary>
    /// Called when the map starts
    /// </summary>
    public interface IMapStart : IEvent
    {
        /// <summary>
        /// Called when the map starts
        /// </summary>
        public void OnMapStart();
    }

    /// <summary>
    /// Called when the map ends
    /// </summary>
    public interface IMapEnd : IEvent
    {
        /// <summary>
        /// Called when the map ends
        /// </summary>
        public void OnMapEnd();
    }


    /// <summary>
    /// Called when the last requested roll is received
    /// </summary>
    public interface IReceiveScore : IEvent
    {
        /// <summary>
        /// Called when the last requested roll is received
        /// </summary>
        public void OnReceiveScore(IScore score);
    }

    /// <summary>
    /// Called when a chat message arrives that has not been handled by any event
    /// </summary>
    public interface IChatMessageReceived : IEvent
    {
        /// <summary>
        /// Called when a chat message arrives that has not been handled by any event
        /// </summary>
        public void OnChatMessageReceived(IChatMessage msg);
    }

    /// <summary>
    /// Slot was updated, it's recommended to use <see cref="IUserSwitchSlot"/> to check if the user switched his slot
    /// </summary>
    public interface ISlotUpdate : IEvent
    {
        /// <summary>
        /// Slot was updated, it's recommended to use <see cref="IUserSwitchSlot"/> to check if the user switched his slot
        /// </summary>
        public void OnSlotUpdate(ISlot slot);
    }

    /// <summary>
    /// Match starts in x seconds
    /// </summary>
    public interface IMatchStartsIn : IEvent
    {
        /// <summary>
        /// Match starts in <paramref name="startDelayS"/> seconds
        /// </summary>
        public void OnMatchStartIn(long startDelayS);
    }

    /// <summary>
    /// Match queued to start in x seconds
    /// </summary>
    public interface IQueueMatchStart : IEvent
    {
        /// <summary>
        /// Match queued to start in <paramref name="startDelayS"/> seconds
        /// </summary>
        public void OnQueueMatchStart(long startDelayS);
    }

    /// <summary>
    /// The match has been aborted
    /// </summary>
    public interface IAbortMatch : IEvent
    {
        /// <summary>
        /// The match has been aborted
        /// </summary>
        public void OnAbortMatch();
    }

    /// <summary>
    /// The host has been changed
    /// </summary>
    public interface IHostChange : IEvent
    {
        /// <summary>
        /// The host has been changed
        /// </summary>
        public void OnHostChange(string newHost);
    }
}
