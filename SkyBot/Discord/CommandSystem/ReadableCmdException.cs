using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Discord.CommandSystem
{
    public class ReadableCmdException : Exception
    {
        public ReadableCmdException(string message) : base(message)
        {

        }

        public ReadableCmdException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
