using AutoRefTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Osu.AutoRef.Data
{
    public class Roll : IEquatable<Roll>, IRoll
    {
        public string Nickname { get; }
        public long Rolled { get; }

        public Roll(string nickname, long rolled)
        {
            Nickname = nickname;
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
                   Rolled == other.Rolled;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nickname, Rolled);
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
        public static implicit operator long(Roll r)
#pragma warning restore CA2225 // Operator overloads have named alternates
        {
            return r.Rolled;
        }
    }
}
