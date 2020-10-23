using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class Roll : IEquatable<Roll>
    {
        public string Nickname { get; }
        public int Min { get; }
        public int Max { get; }
        public int Rolled { get; }

        public Roll(string nickname, int min, int max, int rolled)
        {
            Nickname = nickname;
            Min = min;
            Max = max;
            Rolled = rolled;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Roll);
        }

        public bool Equals([AllowNull] Roll other)
        {
            return other != null &&
                   Nickname == other.Nickname &&
                   Min == other.Min &&
                   Max == other.Max &&
                   Rolled == other.Rolled;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nickname, Min, Max, Rolled);
        }

        public static bool operator ==(Roll left, Roll right)
        {
            if (left == null ||
                right == null)
                return false;

            return left.Rolled == right.Rolled;
        }

        public static bool operator !=(Roll left, Roll right)
        {
            return !(left == right);
        }

        public static bool operator >(Roll left, Roll right)
        {
            if (left == null ||
                right == null)
                return false;

            return left.Rolled > right.Rolled;
        }

        public static bool operator <(Roll left, Roll right)
        {
            if (left == null ||
                right == null)
                return false;

            return left.Rolled < right.Rolled;
        }

#pragma warning disable CA2225 // Operator overloads have named alternates
        public static implicit operator int(Roll r)
#pragma warning restore CA2225 // Operator overloads have named alternates
        {
            return r.Rolled;
        }
    }
}
