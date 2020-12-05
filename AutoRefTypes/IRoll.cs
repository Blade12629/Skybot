using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface IRoll
    {
        public string Nickname { get; }
        public long Min { get; }
        public long Max { get; }
        public long Rolled { get; }
    }
}
