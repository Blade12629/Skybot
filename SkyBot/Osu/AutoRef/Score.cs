using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class Score
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
    }
}
