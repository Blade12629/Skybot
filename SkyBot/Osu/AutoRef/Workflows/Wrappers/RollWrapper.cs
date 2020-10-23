using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflows.Wrappers
{
    public class RollWrapper
    {
        public string Nickname { get; }
        public int Min { get; }
        public int Max { get; }
        public int Rolled { get; }

        internal RollWrapper(string nickname, int min, int max, int rolled)
        {
            Nickname = nickname;
            Min = min;
            Max = max;
            Rolled = rolled;
        }

        public static implicit operator RollWrapper(Roll roll)
        {
            return new RollWrapper(roll.Nickname, roll.Min, roll.Max, roll.Rolled);
        }
    }
}
