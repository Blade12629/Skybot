using DSharpPlus.Entities;
using SkyBot.Database.Models;
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
        /// <summary>
        /// Empty list if no parameters (never null)
        /// </summary>
        public List<string> Parameters { get; }
        /// <summary>
        /// <see cref="string.Empty"/> if no parameters
        /// </summary>
        public string ParameterString { get; set; }

        /// <summary>
        /// Null if private message or config not setup
        /// </summary>
        public DiscordGuildConfig Config { get; set; }

        public CommandEventArg(DiscordGuild guild, DiscordChannel channel, DiscordUser user,
                               DiscordMember member, DiscordMessage message,
                               AccessLevel accessLevel, List<string> parameters, string parameterString,
                               DiscordGuildConfig config)
        {
            Guild = guild;
            Channel = channel;
            User = user;
            Member = member;
            Message = message;
            AccessLevel = accessLevel;
            Parameters = parameters;
            ParameterString = parameterString;
            Config = config;
        }
    }
}
