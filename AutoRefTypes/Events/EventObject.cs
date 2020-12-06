using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace AutoRefTypes.Events
{
    public abstract class EventObject : IEquatable<EventObject>
    {
        public Guid Id { get; set; }
        public bool IsActive { get; set; }

        IEventRunner _evRunner;

        public EventObject(IEventRunner evRunner)
        {
            Id = Guid.NewGuid();
            _evRunner = evRunner;
            _evRunner.Register(this);
        }

        public void Register(IEventRunner eventRunner)
        {
            while (eventRunner.Contains(Id))
                Id = Guid.NewGuid();

            eventRunner.Register(this);
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
