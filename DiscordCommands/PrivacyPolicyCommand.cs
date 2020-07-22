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
                   "The following data is collected:\n" +
                   "Your Discord user data (username, ID, mention, roles)\n" +
                   "Your osu! data (username, ID, Scores)\n" +
                   "osu! multiplayer chat logs\n" +
                   "This data is used to verify users, analyze tournament matches, deliver tourney stats and improve the general discord user feeling\n" +
                   "\n\n" +
                   "If you have any questions or want your data deleted contact me on discord: ??????#0284\n" +
                   "I will try to answer withing 48 hours\n" +
                   "```";
        }
    }
}
