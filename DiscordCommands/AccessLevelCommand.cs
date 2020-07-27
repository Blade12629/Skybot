using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
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

        public string Usage => "!accesslevel\n" +
                               "!accesslevel <userId>\n" +
                               "Admin:\n" +
                               "!accesslevel <userId> <new level (User, VIP, Moderator, Admin, Host, Dev)>";

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Channel.SendMessageAsync($"{args.User.Mention} your access level is {Program.DiscordHandler.CommandHandler.GetAccessLevel(args.User, args.Guild)}");
                return;
            }

            if (!ulong.TryParse(args.Parameters[0], out ulong uid))
            {
                HelpCommand.ShowHelp(args.Channel, this, "Failed to parse user id " + args.Parameters[0]);
                return;
            }

            var duser = Program.DiscordHandler.Client.GetUserAsync(uid).Result;
            AccessLevel access = Program.DiscordHandler.CommandHandler.GetAccessLevel(duser, args.Guild);

            if (args.Parameters.Count == 1)
            {
                args.Channel.SendMessageAsync($"{duser.Username}s access level is {access}");
                return;
            }
            else if (args.AccessLevel < AccessLevel.Admin)
            {
                HelpCommand.ShowHelp(args.Channel, this);
                return;
            }
            

            AccessLevel newAccess = AccessLevel.User;
            if (int.TryParse(args.Parameters[0], out int al))
                newAccess = (AccessLevel)al;
            else if (Enum.TryParse<AccessLevel>(args.Parameters[0], out newAccess))
            {

            }
            else
            {
                HelpCommand.ShowHelp(args.Channel, this, "Failed to parse access level");
                return;
            }

            switch(newAccess)
            {
                case AccessLevel.Dev:
                    args.Channel.SendMessageAsync("Dev permission can only be set via db");
                    return;

                case AccessLevel.Host:
                    if (args.AccessLevel < AccessLevel.Dev)
                    {
                        args.Channel.SendMessageAsync("You need to be atleast Dev to set someone to host!");
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

            //TODO: add command to bind discord role to permission
            //TODO: add command to create embeds with token parser or json
            //TODO: add localizations via resource file

            //TODO: add top list for players and teams
            //      - !stats p/player p/profile osuUserId/osuUsername
            //      - !stats p/player t/top
            //      - !stats p/player l/last
            //      - !stats p/player <page>
            //      - !stats t/team p/profile osuUserId/osuUsername
            //      - !stats t/team t/top
            //      - !stats t/team l/last
            //      - !stats t/team <page>
            //      - !stats m/match <matchId>
            //      - !stats m/match <team a> vs <team b>


        }
    }
}
