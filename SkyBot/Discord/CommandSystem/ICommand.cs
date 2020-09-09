﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Discord.CommandSystem
{
    public interface ICommand
    {
        bool IsDisabled { get; set; }
        /// <summary>
        /// Command Name
        /// </summary>
        string Command { get; }
        /// <summary>
        /// The default access level
        /// </summary>
        AccessLevel AccessLevel { get; }
        bool AllowOverwritingAccessLevel { get; }
        /// <summary>
        /// Public, private chat or both
        /// </summary>
        CommandType CommandType { get; }
        string Description { get; }
        /// <summary>
        /// How to use the command
        /// </summary>
        string Usage { get; }
        /// <summary>
        /// Amount of parameters atleast required
        /// </summary>
        int MinParameters { get; }

        void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args);
    }
}
