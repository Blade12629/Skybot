using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface IChatMessage
    {
        public string From { get; }
        public string Message { get; }
    }
}
