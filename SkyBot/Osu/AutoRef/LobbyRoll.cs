using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class LobbyRoll
    {
        public string Nickname { get; }
        public int Min { get; }
        public int Max { get; }
        public int Rolled { get; }

        public LobbyRoll(string nickname, int min, int max, int rolled)
        {
            Nickname = nickname;
            Min = min;
            Max = max;
            Rolled = rolled;
        }
    }
}
