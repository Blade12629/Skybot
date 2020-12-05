using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface IPick
    {
        public string Nickname { get; }
        public ulong Beatmap { get; }
    }
}
