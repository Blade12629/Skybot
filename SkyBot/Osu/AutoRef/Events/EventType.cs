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
        ChatMessage,
        /// <summary>
        /// Slot was updated
        /// </summary>
        SlotUpdate,
        /// <summary>
        /// BanchoBot: Match starts in 1 second
        /// </summary>
        MatchStartIn,
        /// <summary>
        /// BanchoBot: Queued the match to start in 1 second
        /// </summary>
        QueueMatchStart,
        /// <summary>
        /// Match was aborted
        /// </summary>
        AbortMatch,
        /// <summary>
        /// The host was changed
        /// </summary>
        HostChange,
    }
}
