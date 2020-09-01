using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class ISOCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "iso";

        public AccessLevel AccessLevel => AccessLevel.User;

        public bool AllowOverwritingAccessLevel => false;

        public CommandType CommandType => CommandType.None;

        public string Description => "Displays a list with all ISO Codes (Country codes). Credit: LeoFLT";

        public string Usage => "{prefix}iso";

        public int MinParameters => 0;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            args.Channel.SendMessageAsync("<https://docs.google.com/spreadsheets/d/1_Y_o3Y9mYKdxbciO5Gr2ucENJXG6l_UlhKU4XLeXvNA/edit#gid=1300289989>").ConfigureAwait(false);
        }
    }
}
