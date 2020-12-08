using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes.Events
{
    /// <summary>
    /// EventRunner, used to run all events
    /// </summary>
    public interface IEventRunner
    {
        /// <summary>
        /// Registers an <see cref="EventObject"/>, use <see cref="EventObject.Register(IEventRunner)"/> instead
        /// </summary>
        [Obsolete("Use EventObject.Register(eventRunner) instead")]
        public void Register(EventObject @obj);
        /// <summary>
        /// Deregisters an <see cref="EventObject"/>
        /// </summary>
        public void Delete(EventObject @obj);
        /// <summary>
        /// Is <paramref name="id"/> currently registered
        /// </summary>
        /// <param name="id">EventObject id</param>
        public bool Contains(Guid id);
    }
}
