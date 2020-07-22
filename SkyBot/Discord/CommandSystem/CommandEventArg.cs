using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Discord.CommandSystem
{

    public class CommandEventArg : EventArgs
    {
        /// <summary>
        /// Null if private message
        /// </summary>
        public DiscordGuild Guild { get; set; }
        public DiscordChannel Channel { get; set; }
        public DiscordUser User { get; set; }

        /// <summary>
        /// Null if private message
        /// </summary>
        public DiscordMember Member { get; set; }
        public DiscordMessage Message { get; set; }
        public AccessLevel AccessLevel { get; set; }
        public List<string> Parameters { get; set; }

        public CommandEventArg(DiscordGuild guild, DiscordChannel channel, DiscordUser user,
                               DiscordMember member, DiscordMessage message,
                               AccessLevel accessLevel, List<string> parameters)
        {
            Guild = guild;
            Channel = channel;
            User = user;
            Member = member;
            Message = message;
            AccessLevel = accessLevel;
            Parameters = parameters;
        }
    }
}
