using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skybot.Web.Wiki
{
    public class InvalidWikiCommandInvokeException : Exception
    {
        public InvalidWikiCommandInvokeException(string msg) : base(msg)
        {

        }

        public override string ToString()
        {
            return $"Failed to invoke wiki command: {Message}";
        }
    }
}
