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

        public string Command => ResourcesCommands.VerifyCommand;

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;


        public string Description => ResourcesCommands.VerifyCommandDescription;

        public string Usage => ResourcesCommands.VerifyCommandUsage;
        public bool AllowOverwritingAccessLevel => false;

        public int MinParameters => 0;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            VerificationManager.StartVerification(args.User);
        }
    }
}
