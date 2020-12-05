using AutoRefTypes.Events;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Timers;

namespace SkyBot.Osu.AutoRef.Events
{
    public class EventRunner : IEventRunner
    {
        public event Action<Exception> OnException;

        Dictionary<Guid, EventRegister> _registers = new Dictionary<Guid, EventRegister>();
        Dictionary<string, Action<EventObject>> _eventActions = EventInterfaceActions.ToDict();
        Timer _eventTimer = new Timer(25)
        {
            AutoReset = true,
        };

        public EventRunner()
        {
            _eventTimer.Elapsed += (s, e) => OnTick();
        }

        public void Start()
        {
            _eventTimer.Start();
        }

        public void Stop()
        {
            _eventTimer.Stop();
        }

        public void Register(EventObject obj)
        {
            Type[] interfaces = obj?.GetType()?.GetInterfaces()?.Where(i => !i.Equals(typeof(IEvent)) &&
                                                                             i.GetInterfaces().Any(i2 => i2.Equals(typeof(IEvent))))
                                                               ?.ToArray() ?? Array.Empty<Type>();

            _registers.Add(obj.Id, new EventRegister(obj, interfaces ?? Array.Empty<Type>()));
        }

        public void Delete(EventObject obj)
        {
            _registers.Remove(obj.Id);
        }

        void OnTick()
        {
            EventRegister[] registers = _registers.Values.ToArray();

            for (int i = 0; i < registers.Length; i++)
                for (int x = 0; x < registers[i].InterfaceTypes.Length; x++)
                    InvokeInterface(registers[i].InterfaceTypes[x], registers[i].Object);
        }

        void InvokeInterface(Type @interface, EventObject obj)
        {
            string name = $"{@interface.Name}Update";

            if (!_eventActions.ContainsKey(name))
            {
                OnException?.Invoke(new Exception("Unable to find interface type to invoke for obj " + obj.Id));
                return;
            }

            _eventActions[name]?.Invoke(obj);
        }
    }
}
