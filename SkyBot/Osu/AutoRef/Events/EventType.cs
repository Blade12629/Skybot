using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Events
{
    public enum EventType
    {
        /// <summary>
        /// Called every tick
        /// </summary>
        Update,

        /// <summary>
        /// User joins lobby
        /// </summary>
        UserJoin,
        /// <summary>
        /// User leaves lobby
        /// </summary>
        UserLeave,
        /// <summary>
        /// User switches/gets moved to another slot
        /// </summary>
        UserSwitchSlot,

        /// <summary>
        /// Map change
        /// </summary>
        MapChange,
        /// <summary>
        /// Map starts
        /// </summary>
        MapStart,
        /// <summary>
        /// Map ends
        /// </summary>
        MapEnd,

        /// <summary>
        /// Received user score
        /// </summary>
        ReceiveScore,

        /// <summary>
        /// All users readied up
        /// </summary>
        AllUsersReady,
        /// <summary>
        /// New chat message in lobby
        /// </summary>
        OnChatMessage,
        /// <summary>
        /// Slot was updated
        /// </summary>
        OnSlotUpdate,
        /// <summary>
        /// Roll was received
        /// </summary>
        OnRollReceive,
    }
}
