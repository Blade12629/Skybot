using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Management
{
    public static class AutoRefManager
    {
        const int MAX_LOBBY_INSTANCES = 4;

        public static bool IsOnMaxInstances
        {
            get
            {
                lock(_syncRoot)
                {
                    return MAX_LOBBY_INSTANCES > _instances.Count;
                }
            }
        }

        static List<AutoRefEngine> _instances;
        static readonly object _syncRoot;

        static AutoRefManager()
        {
            _instances = new List<AutoRefEngine>();
            _syncRoot = new object();
        }

        public static AutoRefEngine CreateInstance(bool autoCreate = false, DateTime autoCreationDate = default)
        {
            lock(_syncRoot)
            {
                if (_instances.Count >= MAX_LOBBY_INSTANCES)
                    return null;

                AutoRefEngine engine = new AutoRefEngine();
                _instances.Add(engine);

                if (autoCreate && !autoCreationDate.Equals(default))
                    engine.StartCreationTimer(autoCreationDate);

                return engine;
            }
        }

        public static void DeregisterInstance(AutoRefEngine engine)
        {
            lock(_syncRoot)
            {
                if (_instances.Count == 0)
                    return;

                _instances.Remove(engine);
            }
        }
    }
}
