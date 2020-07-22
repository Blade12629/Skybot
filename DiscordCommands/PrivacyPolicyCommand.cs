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

        public string Command => "privacy";

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        public string Description => "Displays the bots privacy policy";

        public string Usage => "!privacy";

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            args.Channel.SendMessageAsync(GetPrivacyPolicy()).Wait();
        }

        private string GetPrivacyPolicy()
        {
            return "Privacy Policy\n```\n" +
                   "Not implemented yet, contact ??????#0284 for more infos\n" +
                   "```";
        }
    }
}
