using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Skybot.Web.Wiki
{
    public class WikiScriptCommand
    {
        public string Command { get; }
        public Func<string, List<string>, string> OnAreaStart { get; }
        public Func<string, List<string>, string> OnAreaEnd { get; }
        public Func<string, List<string>, string> OnSingle { get; }

        public WikiScriptCommand(string command, Func<string, List<string>, string> onAreaStart, Func<string, List<string>, string> onAreaEnd, Func<string, List<string>, string> onSingle)
        {
            Command = command;
            OnAreaStart = onAreaStart;
            OnAreaEnd = onAreaEnd;
            OnSingle = onSingle;
        }
    }
}
