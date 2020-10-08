using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot.Osu.AutoRef.Match
{
    public class MapPool
    {
        Dictionary<Mods, List<long>> _pool;

        public MapPool()
        {
            _pool = new Dictionary<Mods, List<long>>();
        }

        public List<long> this[Mods m]
            => GetMaps(m);

        public Mods? this[long m]
        {
            get
            {
                if (TryGetMods(m, out Mods mods))
                    return mods;

                return null;
            }
        }

        public bool TryGetMods(long map, out Mods mods)
        {
            for (int i = 0; i < _pool.Count; i++)
            {
                List<long> maps = _pool.Values.ElementAt(i);

                if (maps.Contains(map))
                {
                    mods = _pool.Keys.ElementAt(i);
                    return true;
                }
            }

            mods = Mods.Nomod;
            return false;
        }

        public List<long> GetMaps(Mods mod)
        {
            if (!_pool.ContainsKey(mod))
                return null;

            return _pool[mod];
        }

        public void AddMap(Mods mod, long map)
        {
            if (AddMod(mod))
            {
                if (!_pool[mod].Contains(map))
                    _pool[mod].Add(map);
            }
            else
                _pool[mod].Add(map);
        }

        public bool AddMod(Mods mod)
        {
            if (_pool.ContainsKey(mod))
                return false;

            _pool.Add(mod, new List<long>());
            return true;
        }
    }
}
