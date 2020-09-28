using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands.Scripting
{
    public class ScriptExitException : Exception
    {
        public ScriptExitException()
        {
        }

        public ScriptExitException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
