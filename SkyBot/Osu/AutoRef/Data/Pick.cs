using AutoRefTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Osu.AutoRef.Data
{
    public class Pick : IPick, IEquatable<Pick>
    {
        public string Nickname { get; }
        public ulong Beatmap { get; }

        public Pick(string nickname, ulong beatmap)
        {
            Nickname = nickname;
            Beatmap = beatmap;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Pick);
        }

        public bool Equals([AllowNull] Pick other)
        {
            return other != null &&
                   Nickname == other.Nickname &&
                   Beatmap == other.Beatmap;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Nickname, Beatmap);
        }

        public static bool operator ==(Pick left, Pick right)
        {
            return EqualityComparer<Pick>.Default.Equals(left, right);
        }

        public static bool operator !=(Pick left, Pick right)
        {
            return !(left == right);
        }
    }
}
