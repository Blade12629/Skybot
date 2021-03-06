﻿using AutoRefTypes;
using AutoRefTypes.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Events
{
    public static class EventHelper
    {
        public static Event CreateUpdateEvent()
        {
            return new Event(typeof(IUpdate), EventType.Update);
        }


        public static Event CreateJoinEvent(string nick, ISlot slot)
        {
            return new Event(typeof(IUserJoin), EventType.UserJoin, nick, slot);
        }

        public static Event CreateLeaveEvent(string nick)
        {
            return new Event(typeof(IUserLeave), EventType.UserLeave, nick);
        }

        public static Event CreateUserSwitchSlotEvent(string nick, ISlot oldSlot, ISlot newSlot)
        {
            return new Event(typeof(IUserSwitchSlot), EventType.UserSwitchSlot, nick, oldSlot, newSlot);
        }


        public static Event CreateMapChangeEvent(ulong newMap)
        {
            return new Event(typeof(IMapChange), EventType.MapChange, newMap);
        }

        public static Event CreateMapStartEvent()
        {
            return new Event(typeof(IMapStart), EventType.MapStart);
        }

        public static Event CreateMapEndEvent()
        {
            return new Event(typeof(IMapEnd), EventType.MapEnd);
        }


        public static Event CreateReceiveScoreEvent(IScore score)
        {
            return new Event(typeof(IReceiveScore), EventType.ReceiveScore, score);
        }

        public static Event CreateAllUsersReadyEvent()
        {
            return new Event(typeof(IAllUsersReady), EventType.AllUsersReady);
        }

        public static Event CreateChatMessageEvent(IChatMessage msg)
        {
            return new Event(typeof(IChatMessageReceived), EventType.ChatMessage, msg);
        }

        public static Event CreateSlotUpdateEvent(ISlot slot)
        {
            return new Event(typeof(ISlotUpdate), EventType.SlotUpdate, slot);
        }

        public static Event CreateMatchStartInEvent(long startDelayS)
        {
            return new Event(typeof(IMatchStartsIn), EventType.MatchStartIn, startDelayS);
        }

        public static Event CreateQueueMatchStartEvent(long startDelayS)
        {
            return new Event(typeof(IQueueMatchStart), EventType.QueueMatchStart, startDelayS);
        }

        public static Event CreateAbortMatchEvent()
        {
            return new Event(typeof(IAbortMatch), EventType.AbortMatch);
        }

        public static Event CreateHostChangeEvent(string newHost)
        {
            return new Event(typeof(IHostChange), EventType.HostChange, newHost);
        }
    }
}
