using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface IWorkflow
    {
        public int Id { get; }
        public bool IsReady { get; }
        public Uri ProfileUrl { get; }
        public string Nickname { get; }
        public SlotColor? Color { get; }
        public string Role { get; }
        public bool IsUsed { get; }

        public List<string> Mods { get; }
    }
}
