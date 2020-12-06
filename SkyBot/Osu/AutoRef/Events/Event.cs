using AutoRefTypes.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Text;

namespace SkyBot.Osu.AutoRef.Events
{
    public class Event : IEquatable<Event>
    {
        /// <summary>
        /// Event Interface Type
        /// </summary>
        public Type InterfaceType { get; }

        /// <summary>
        /// Event Type
        /// </summary>
        public EventType Type { get; }

        /// <summary>
        /// Event Method Parameter Data
        /// </summary>
        public object[] EventData { get; }

        /// <summary>
        /// Event Method
        /// </summary>
        public MethodInfo EventMethod { get; }

        public Event(Type interfaceType, EventType type, params object[] eventData)
        {
            InterfaceType = interfaceType;
            Type = type;
            EventData = eventData;
            EventMethod = interfaceType.GetMethods()[0];
        }

        public void Invoke(EventObject eobj)
        {
            EventMethod.Invoke(eobj, EventData);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Event);
        }

        public bool Equals([AllowNull] Event other)
        {
            return other != null &&
                   Type == other.Type;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Type);
        }

        public static bool operator ==(Event left, Event right)
        {
            return EqualityComparer<Event>.Default.Equals(left, right);
        }

        public static bool operator !=(Event left, Event right)
        {
            return !(left == right);
        }
    }
}
