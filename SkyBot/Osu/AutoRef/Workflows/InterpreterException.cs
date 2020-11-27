using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Workflows
{
    public class InterpreterException : Exception
    {
        public InterpreterException(string type, string message) : this($"{type}:\t{message}")
        {
        }

        private InterpreterException()
        {
        }

        private InterpreterException(string message) : base(message)
        {
        }

        private InterpreterException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
