using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefScripts
{
    public class ScriptInput
    {
        public string CaptainA { get; set; }
        public string CaptainB { get; set; }

        public ScriptInput(string captainA, string captainB)
        {
            CaptainA = captainA;
            CaptainB = captainB;
        }

        public ScriptInput()
        {
        }
    }
}
