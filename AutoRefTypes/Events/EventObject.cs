using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AutoRefTypes.Events
{
    /// <summary>
    /// Event object, used to access all events
    /// </summary>
    public abstract class EventObject : IEquatable<EventObject>
    {
        /// <summary>
        /// Object id
        /// </summary>
        public Guid Id { get; set; }
        /// <summary>
        /// Is currently active
        /// </summary>
        public bool IsActive { get; set; }

        IEventRunner _evRunner;

        /// <summary>
        /// Event object, used to access all events
        /// </summary>
        public EventObject(IEventRunner evRunner, bool register = true)
        {
            Id = Guid.NewGuid();
            _evRunner = evRunner;

            if (register)
#pragma warning disable CS0618 // Type or member is obsolete
                _evRunner.Register(this);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        /// <summary>
        /// Registers the <see cref="EventObject"/>
        /// </summary>
        /// <param name="eventRunner"></param>
        public void Register(IEventRunner eventRunner)
        {
            while (eventRunner.Contains(Id))
                Id = Guid.NewGuid();

#pragma warning disable CS0618 // Type or member is obsolete
            eventRunner.Register(this);
#pragma warning restore CS0618 // Type or member is obsolete
        }

        public void Deregister(IEventRunner eventRunner)
        {
            _evRunner.Delete(this);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as EventObject);
        }

        public bool Equals([AllowNull] EventObject other)
        {
            return other != null &&
                   Id.Equals(other.Id);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        public static bool operator ==(EventObject left, EventObject right)
        {
            return EqualityComparer<EventObject>.Default.Equals(left, right);
        }

        public static bool operator !=(EventObject left, EventObject right)
        {
            return !(left == right);
        }
    }
}
