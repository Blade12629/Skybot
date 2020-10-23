using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Osu.AutoRef.Chat
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

        public bool StartsWith(string source)
        {
            return source?.StartsWith(MessageStart, StringComparison.CurrentCultureIgnoreCase) ?? false;
        }

        public bool IsFromUser(string user)
        {
            return Nickname.Equals(user, StringComparison.CurrentCultureIgnoreCase);
        }
    }
}
