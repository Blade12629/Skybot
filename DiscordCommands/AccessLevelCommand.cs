using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace DiscordCommands
{
    public class AccessLevelCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "accesslevel";

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Shows or sets your accesslevel";

        public string Usage => "{prefix}accesslevel\n" + 
                               "{prefix}accesslevel <userId>\n" +
                               "Admin:\n" +
                               "{prefix}accesslevel <userId> <new level (User, VIP, Moderator, Admin, Host, Dev)>";

        public bool AllowOverwritingAccessLevel => false;

        public int MinParameters => 0;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Channel.SendMessageAsync($"{args.User.Mention} your access level is {Program.DiscordHandler.CommandHandler.GetAccessLevel(args.User, args.Guild)}");
                return;
            }

            if (!ulong.TryParse(args.Parameters[0], out ulong uid))
            {
                HelpCommand.ShowHelp(args.Channel, this, string.Format(ResourceExceptions.FailedParseException, args.Parameters[0]));
                return;
            }

            var duser = client.GetUserAsync(uid).Result;
            AccessLevel access = Program.DiscordHandler.CommandHandler.GetAccessLevel(duser, args.Guild);

            if (args.Parameters.Count == 1)
            {
                args.Channel.SendMessageAsync($"Access level of user {duser.Username} is {access}");
                return;
            }
            else if (args.AccessLevel < AccessLevel.Admin)
            {
                HelpCommand.ShowHelp(args.Channel, this);
                return;
            }


            AccessLevel newAccess;
            if (args.Parameters[1].TryParseEnum(out AccessLevel acc))
                newAccess = acc;
            else
            {
                HelpCommand.ShowHelp(args.Channel, this, string.Format(ResourceExceptions.FailedParseException, "Access Level"));
                return;
            }

            switch(newAccess)
            {
                case AccessLevel.Dev:
                    args.Channel.SendMessageAsync("Dev permission can only be set via db");
                    return;

                case AccessLevel.Host:
                    if (args.AccessLevel < AccessLevel.Host)
                    {
                        args.Channel.SendMessageAsync("You need to be atleast Host to set someone to host!");
                        return;
                    }
                    break;

                case AccessLevel.Admin:
                    if (args.AccessLevel < AccessLevel.Host)
                    {
                        args.Channel.SendMessageAsync("Only the host can add more admins!");
                        return;
                    }
                    break;

                default:
                    break;
            }

            CommandHandler.SetAccessLevel(uid, args.Guild.Id, newAccess);

            args.Channel.SendMessageAsync($"Set {uid} from {access} to {newAccess}");
        }
    }
}
