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

        public string Usage => "!verify";

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            VerificationManager.StartVerification(args.User);
        }
    }
}
