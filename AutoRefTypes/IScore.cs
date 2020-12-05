using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface IScore
    {
        public string Username { get; }
        public long UserScore { get; }
        public bool Passed { get; }
    }
}
