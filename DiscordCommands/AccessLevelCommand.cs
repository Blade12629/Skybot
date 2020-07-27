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

        public string Command => ResourcesCommands.AccessLevelCommand;

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.Public;

        public string Description => ResourcesCommands.AccessLevelDescription;

        public string Usage => ResourcesCommands.AccessLevelUsage;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Channel.SendMessageAsync(string.Format(CultureInfo.CurrentCulture, ResourcesCommands.AccessLevelUser, args.User.Mention, Program.DiscordHandler.CommandHandler.GetAccessLevel(args.User, args.Guild)));
                return;
            }

            if (!ulong.TryParse(args.Parameters[0], out ulong uid))
            {
                HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, Resources.FailedParseException, args.Parameters[0]));
                return;
            }

            var duser = Program.DiscordHandler.Client.GetUserAsync(uid).Result;
            AccessLevel access = Program.DiscordHandler.CommandHandler.GetAccessLevel(duser, args.Guild);

            if (args.Parameters.Count == 1)
            {
                args.Channel.SendMessageAsync(string.Format(CultureInfo.CurrentCulture, ResourcesCommands.AccessLevelUserOther, duser.Username, access));
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
                HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, Resources.FailedParseException, Resources.AccessLevel));
                return;
            }

            switch(newAccess)
            {
                case AccessLevel.Dev:
                    args.Channel.SendMessageAsync(ResourcesCommands.AccessLevelSetDevPermission);
                    return;

                case AccessLevel.Host:
                    if (args.AccessLevel < AccessLevel.Dev)
                    {
                        args.Channel.SendMessageAsync(ResourcesCommands.AccessLevelDevOnlyAddHost);
                        return;
                    }
                    break;

                case AccessLevel.Admin:
                    if (args.AccessLevel < AccessLevel.Host)
                    {
                        args.Channel.SendMessageAsync(ResourcesCommands.AccessLevelHostOnlyAddAdmins);
                        return;
                    }
                    break;

                default:
                    break;
            }

            CommandHandler.SetAccessLevel(uid, args.Guild.Id, newAccess);

            args.Channel.SendMessageAsync(string.Format(CultureInfo.CurrentCulture, ResourcesCommands.AccessLevelSetPermission, uid, access, newAccess));

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
