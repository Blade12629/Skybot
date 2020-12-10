using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface IDiscordHandler
    {
        public void SendMessage(ulong channel, string message);
        public void SendEmbed(ulong channel, string title, string description);
        public void SendEmbed(ulong channel, string title, string description, params (string, string, bool)[] fields);
    }
}
