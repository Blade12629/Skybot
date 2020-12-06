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
    public class EventRunner : IEventRunner
    {
        Dictionary<Guid, EventRegister> _registers;
        Queue<Event> _eventQueue;
        readonly object _syncRoot = new object();

        public EventRunner()
        {
            _registers = new Dictionary<Guid, EventRegister>();
            _eventQueue = new Queue<Event>();
        }

        /// <summary>
        /// Registers an <see cref="EventObject"/>
        /// </summary>
        public void Register(EventObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            Type[] interfaces = obj.GetType().GetInterfaces()?.Where(i => !i.Equals(typeof(IEvent)) &&
                                                                           i.GetInterfaces().Any(i2 => i2.Equals(typeof(IEvent))))
                                                             ?.ToArray() ?? Array.Empty<Type>();

            _registers.Add(obj.Id, new EventRegister(obj, interfaces ?? Array.Empty<Type>()));
        }


        public bool Contains(Guid id)
        {
            return _registers.ContainsKey(id);
        }

        /// <summary>
        /// Deregisters an <see cref="EventObject"/>
        /// </summary>
        public void Delete(EventObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException(nameof(obj));

            _registers.Remove(obj.Id);
        }

        /// <summary>
        /// Enqueues a single <see cref="Event"/>
        /// </summary>
        public void EnqueueEvent(Event ev)
        {
            lock(_syncRoot)
            {
                if (ev == null)
                    throw new ArgumentNullException(nameof(ev));

                _eventQueue.Enqueue(ev);
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

                for (int i = 0; i < events.Count; i++)
                    _eventQueue.Enqueue(events[i]);
            }
        }

        /// <summary>
        /// Run one event cycle
        /// </summary>
        public void OnTick()
        {
            lock(_syncRoot)
            {
                EventRegister[] registers = _registers.Values.ToArray();

                _eventQueue.Enqueue(EventHelper.CreateUpdateEvent());

                while (_eventQueue.TryDequeue(out Event ev))
                {
                    IEnumerable<EventRegister> cregs = registers.Where(r => r.InterfaceTypes.Any(it => it.Equals(ev.InterfaceType)));

                    foreach (EventRegister er in cregs)
                    {
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
        }
    }
}
