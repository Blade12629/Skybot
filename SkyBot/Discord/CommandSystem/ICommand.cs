using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Discord.CommandSystem
{
    public interface ICommand
    {
        bool IsDisabled { get; set; }
        string Command { get; }
        AccessLevel AccessLevel { get; }
        CommandType CommandType { get; }
        string Description { get; }
        string Usage { get; }

        void Invoke(CommandHandler handler, CommandEventArg args);
    }
}
