using AutoRefTypes;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Chat
{
    public class ChatMessage : IChatMessage
    {
        public string From { get; }
        public string Message { get; }

        public ChatMessage(string from, string message)
        {
            From = from;
            Message = message;
        }
    }
}
