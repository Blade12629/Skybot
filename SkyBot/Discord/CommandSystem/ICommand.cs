using System;
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
        AccessLevel AccessLevel { get; }
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

        void Invoke(CommandHandler handler, CommandEventArg args);
    }
}
