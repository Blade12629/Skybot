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

        public string Description => ResourcesCommands.AccessLevelCommandDescription;

        public string Usage => ResourcesCommands.AccessLevelCommandUsage;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count == 0)
            {
                args.Channel.SendMessageAsync(string.Format(CultureInfo.CurrentCulture, ResourcesCommands.AccessLevelCommandUser, args.User.Mention, Program.DiscordHandler.CommandHandler.GetAccessLevel(args.User, args.Guild)));
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
                args.Channel.SendMessageAsync(string.Format(CultureInfo.CurrentCulture, ResourcesCommands.AccessLevelCommandUserOther, duser.Username, access));
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
                HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, Resources.FailedParseException, Resources.AccessLevel));
                return;
            }

            switch(newAccess)
            {
                case AccessLevel.Dev:
                    args.Channel.SendMessageAsync(ResourcesCommands.AccessLevelCommandSetDevPermission);
                    return;

                case AccessLevel.Host:
                    if (args.AccessLevel < AccessLevel.Host)
                    {
                        args.Channel.SendMessageAsync(ResourcesCommands.AccessLevelCommandHostOnlyAddHost);
                        return;
                    }
                    break;

                case AccessLevel.Admin:
                    if (args.AccessLevel < AccessLevel.Host)
                    {
                        args.Channel.SendMessageAsync(ResourcesCommands.AccessLevelCommandHostOnlyAddAdmins);
                        return;
                    }
                    break;

                default:
                    break;
            }

            CommandHandler.SetAccessLevel(uid, args.Guild.Id, newAccess);

            args.Channel.SendMessageAsync(string.Format(CultureInfo.CurrentCulture, ResourcesCommands.AccessLevelCommandSetPermission, uid, access, newAccess));
        }
    }
}
