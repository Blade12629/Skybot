using AutoRefTypes.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Events
{
    public static class EventInterfaceActions
    {
        public static Dictionary<string, Action<EventObject>> ToDict()
        {
            Dictionary<string, Action<EventObject>> actions = new Dictionary<string, Action<EventObject>>();

            AddAction(typeof(ITickUpdate), e => ((ITickUpdate)e).UpdateTick());
            AddAction(typeof(IUpdateEvent), e => ((IUpdateEvent)e).Update());

            return actions;

            void AddAction(Type type, Action<EventObject> ac)
            {
                string name = $"{type.Name}Update";
                actions.Add(name, ac);
            }
        }

        public interface ITickUpdate
        {
            public void UpdateTick();
        }
    }
}
