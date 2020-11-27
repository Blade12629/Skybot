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

        public string Usage => "{prefix}privacy";

        public bool AllowOverwritingAccessLevel => false;

        public int MinParameters => 0;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            args.Channel.SendMessageAsync(GetPrivacyPolicy()).Wait();
        }

        private string GetPrivacyPolicy()
        {
            return
@"Privacy Policy
```
The following data is collected:
Your Discord user data (username, ID, mention, roles)
Your osu! data (username, ID, Scores)
This data is used to verify users, analyze tournament matches, deliver tourney stats and improve the general discord user feeling


If you have any questions or want your data deleted contact me on discord: ??????#0284
I will try to answer withing 48 hours
(If you request the deletion of your data, your data will be deleted and you will be blacklisted from the bot for atleast 1 month)
```";
        }
    }
}
