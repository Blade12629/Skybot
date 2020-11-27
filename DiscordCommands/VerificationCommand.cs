using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class VerificationCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "verify";

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;


        public string Description => "Verify yourself";

        public string Usage => "{prefix}verify";

        public bool AllowOverwritingAccessLevel => false;

        public int MinParameters => 0;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            VerificationManager.StartVerification(args.User);
        }
    }
}
