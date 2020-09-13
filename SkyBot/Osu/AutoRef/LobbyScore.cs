using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class LobbyScore
    {
        public string Username { get; }
        public long Score { get; }
        public bool Passed { get; }

        public LobbyScore(string username, long score, bool passed)
        {
            Username = username;
            Score = score;
            Passed = passed;
        }
    }
}
