using AutoRefTypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Osu.AutoRef.Data
{
    public class Score : IScore, IEquatable<Score>
    {
        public string Username { get; }
        public long UserScore { get; }
        public bool Passed { get; }

        public Score(string username, long score, bool passed)
        {
            Username = username;
            UserScore = score;
            Passed = passed;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Score);
        }

        public bool Equals([AllowNull] Score other)
        {
            return other != null &&
                   Username == other.Username &&
                   UserScore == other.UserScore &&
                   Passed == other.Passed;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Username, UserScore, Passed);
        }

        public static bool operator ==(Score left, Score right)
        {
            return EqualityComparer<Score>.Default.Equals(left, right);
        }

        public static bool operator !=(Score left, Score right)
        {
            return !(left == right);
        }
    }
}
