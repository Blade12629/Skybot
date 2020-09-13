using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class ChatInteraction
    {
        public string Nickname { get; }
        public string MessageStart { get; }
        public Action<string> Action { get; }

        public ChatInteraction(string nickname, string messageStart, Action<string> action)
        {
            Nickname = nickname;
            MessageStart = messageStart;
            Action = action;
        }
    }
}
