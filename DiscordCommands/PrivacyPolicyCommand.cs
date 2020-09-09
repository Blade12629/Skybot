using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class PrivacyPolicyCommand : ICommand
    {
        public bool IsDisabled
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public string Command => ResourcesCommands.PrivacyPolicyCommand;

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        public string Description => ResourcesCommands.PrivacyPolicyCommandDescription;

        public string Usage => ResourcesCommands.PrivacyPolicyCommandUsage;
        public bool AllowOverwritingAccessLevel => false;

        public int MinParameters => 0;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            args.Channel.SendMessageAsync(GetPrivacyPolicy()).Wait();
        }

        private string GetPrivacyPolicy()
        {
            return ResourcesCommands.PrivacyPolicyCommandPrivacyPolicy;
        }
    }
}
