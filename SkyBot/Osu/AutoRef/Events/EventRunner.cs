using AutoRefTypes.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Timers;

namespace SkyBot.Osu.AutoRef.Events
{
    /// <summary>
    /// EventRunner, used to run all events
    /// </summary>
    public class EventRunner : IEventRunner
    {
        Dictionary<Guid, EventRegister> _registers;
        List<Event> _eventQueue;
        readonly object _syncRoot = new object();
        bool _lastTick;

        public EventRunner()
        {
            _registers = new Dictionary<Guid, EventRegister>();
            _eventQueue = new List<Event>();
        }

        /// <summary>
        /// Registers an <see cref="EventObject"/>, use <see cref="EventObject.Register(IEventRunner)"/> instead
        /// </summary>
        [Obsolete("Use EventObject.Register(eventRunner) instead")]
        public void Register(EventObject obj)
        {
            lock(_syncRoot)
            {
                if (obj == null)
                    throw new ArgumentNullException(nameof(obj));

                Type[] interfaces = obj.GetType().GetInterfaces()?.Where(i => !i.Equals(typeof(IEvent)) &&
                                                                               i.GetInterfaces().Any(i2 => i2.Equals(typeof(IEvent))))
                                                                 ?.ToArray() ?? Array.Empty<Type>();

                _registers.Add(obj.Id, new EventRegister(obj, interfaces ?? Array.Empty<Type>()));
            }
        }

        /// <summary>
        /// Is <paramref name="id"/> currently registered
        /// </summary>
        /// <param name="id">EventObject id</param>
        public bool Contains(Guid id)
        {
            lock (_syncRoot)
            {
                return _registers.ContainsKey(id);
            }
        }

        /// <summary>
        /// Deregisters an <see cref="EventObject"/>
        /// </summary>
        public void Delete(EventObject obj)
        {
            lock (_syncRoot)
            {
                if (obj == null)
                throw new ArgumentNullException(nameof(obj));

                _registers.Remove(obj.Id);
            }
        }

        /// <summary>
        /// Enqueues a single <see cref="Event"/>
        /// </summary>
        public void EnqueueEvent(Event ev)
        {
            lock (_syncRoot)
            {
                if (ev == null)
                    throw new ArgumentNullException(nameof(ev));

                _eventQueue.Add(ev);
            }
        }

        /// <summary>
        /// Enqueues multiple <see cref="Event"/>
        /// </summary>
        public void EnqueueEvents(List<Event> events)
        {
            lock (_syncRoot)
            {
                if (events == null)
                    throw new ArgumentNullException(nameof(events));

                _eventQueue.AddRange(events);
            }
        }

        public void Clear()
        {
            _registers?.Clear();
            _eventQueue?.Clear();
            _lastTick = false;
        }

        /// <summary>
        /// Run one event cycle
        /// </summary>
        public void OnTick()
        {
            lock (_syncRoot)
            {
              if (_lastTick)
                {
                    _lastTick = !_lastTick;
                    return;
                }

                _lastTick = !_lastTick;

                EventRegister[] registers = _registers.Values.ToArray();

                List<Event> eventQueue = _eventQueue;
                //Do update event as the last event
                eventQueue.Add(EventHelper.CreateUpdateEvent());

                foreach (Event ev in eventQueue)
                {
                    foreach(EventRegister er in registers)
                    {
                        if (er.InterfaceTypes.Any(it => it.Equals(ev.InterfaceType)))
                        {
                            if (ev.InterfaceType.Equals(typeof(IMapEnd)))
                                Logger.Log("IUserSwitchSlot event", LogLevel.Info);

                            try
                            {
                                ev.Invoke(er.Object);
                            }
                            catch (Exception ex)
                            {
                                Logger.Log(ex, LogLevel.Error);
                            }
                        }
                    }
                }

                _eventQueue = new List<Event>();
            }
        }
    }
}
