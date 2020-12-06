using AutoRefTypes.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Osu.AutoRef.Events
{
    public class EventRegister : IEquatable<EventRegister>
    {
        /// <summary>
        /// Event Object
        /// </summary>
        public EventObject Object { get; }
        /// <summary>
        /// Event Interface Types
        /// </summary>
        public Type[] InterfaceTypes { get; }

        public EventRegister(EventObject @object, Type[] interfaceTypes)
        {
            Object = @object;
            InterfaceTypes = interfaceTypes;
        }

        public override bool Equals(object obj)
        {
            return obj is EventRegister register && Equals(register);
        }

        public bool Equals([AllowNull] EventRegister other)
        {
            return EqualityComparer<EventObject>.Default.Equals(Object, other.Object);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Object);
        }

        public static bool operator ==(EventRegister left, EventRegister right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(EventRegister left, EventRegister right)
        {
            return !(left == right);
        }
    }
}
