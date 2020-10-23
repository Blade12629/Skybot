using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflows.Wrappers
{
    public class ScoreWrapper
    {
        public string Username { get; }
        public long UserScore { get; }
        public bool Passed { get; }

        internal ScoreWrapper(string username, long score, bool passed)
        {
            Username = username;
            UserScore = score;
            Passed = passed;
        }

        public static implicit operator ScoreWrapper(Score s)
        {
            return new ScoreWrapper(s.Username, s.UserScore, s.Passed);
        }
    }
}
