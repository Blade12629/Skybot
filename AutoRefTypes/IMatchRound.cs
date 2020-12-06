using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface IMatchRound
    {
        public int Round { get; }
        public ulong Beatmap { get; }
        public IReadOnlyList<IScore> Scores { get; }
    }
}
