using AutoRefTypes.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Events
{
    public class TickEvent : EventObject, EventInterfaceActions.ITickUpdate
    {
        LobbyController _lc;
        AutoRefController _arc;

        private TickEvent(IEventRunner runner, LobbyController lc, AutoRefController arc) : base(runner)
        {
            _lc = lc;
            _arc = arc;
        }

        public void UpdateTick()
        {
            _lc.OnTick();
            _arc.OnTick();
        }

        public static TickEvent TickEventObject { get; private set; }

        public static void Initialize(EventRunner eventRunner, AutoRefController arc, LobbyController lc)
        {
            TickEventObject = new TickEvent(eventRunner, lc, arc);
            eventRunner.Register(TickEventObject);
        }
    }
}
