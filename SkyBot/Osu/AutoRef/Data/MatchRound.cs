using AutoRefTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Osu.AutoRef.Data
{
    public class MatchRound : IMatchRound, IEquatable<MatchRound>
    {
        public int Round { get; }

        public ulong Beatmap { get; }

        public IReadOnlyList<IScore> Scores { get; }

        public MatchRound(int round, ulong beatmap, IReadOnlyList<IScore> scores)
        {
            Round = round;
            Beatmap = beatmap;
            Scores = scores;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as MatchRound);
        }

        public bool Equals([AllowNull] MatchRound other)
        {
            return other != null &&
                   Round == other.Round;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Round);
        }

        public static bool operator ==(MatchRound left, MatchRound right)
        {
            return EqualityComparer<MatchRound>.Default.Equals(left, right);
        }

        public static bool operator !=(MatchRound left, MatchRound right)
        {
            return !(left == right);
        }
    }
}
