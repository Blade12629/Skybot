using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class PermissionCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => ResourcesCommands.PermissionCommand;

        public AccessLevel AccessLevel => AccessLevel.Admin;

        public CommandType CommandType => CommandType.Public;

        public string Description => ResourcesCommands.PermissionCommandDescription;

        public string Usage => ResourcesCommands.PermissionCommandUsage;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count < 2)
            {
                HelpCommand.ShowHelp(args.Channel, this);
                return;
            }

            if (!ulong.TryParse(args.Parameters[1], out ulong roleId))
            {
                HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, Resources.FailedParseException, args.Parameters[1]));
                return;
            }
            else if (args.Guild.GetRole(roleId) == null)
            {
                HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, ResourcesCommands.PermissionCommandRoleNotFound, args.Parameters[1]));
                return;
            }

            AccessLevel? access = null;
            if (args.Parameters.Count > 2)
            {
                if (args.Parameters[2].TryParseEnum(out AccessLevel acc))
                    access = acc;
                else
                {
                    HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, ResourcesCommands.PermissionCommandAccessLevelNotFound, args.Parameters[2]));
                    return;
                }

                if (access.HasValue)
                {
                    switch(access.Value)
                    {
                        case AccessLevel.Dev:
                            args.Channel.SendMessageAsync(ResourcesCommands.PermissionCommandInsufficientPermission);
                            return;

                        case AccessLevel.Admin:
                            if (args.AccessLevel < AccessLevel.Host)
                            {
                                args.Channel.SendMessageAsync(ResourcesCommands.PermissionCommandInsufficientPermission);
                                return;
                            }
                            break;
                    }
                }
            }


            switch (args.Parameters[0].ToLower(CultureInfo.CurrentCulture))
            {
                case "bind":
                    if (!access.HasValue)
                    {
                        HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, ResourcesCommands.PermissionCommandAccessLevelNotFound, args.Parameters[2]));
                        return;
                    }

                    if (CommandHandler.BindPermssion(args.Guild, roleId, access.Value))
                        args.Channel.SendMessageAsync("Binded permission");
                    else
                        args.Channel.SendMessageAsync("Already binded or failed to bind");

                    break;

                case "unbind":
                    if (CommandHandler.UnbindPermission(args.Guild, roleId, access))
                        args.Channel.SendMessageAsync("Unbinded permission");
                    else
                        args.Channel.SendMessageAsync("Already unbinded or failed to unbind");
                    break;
            }
        }
    }
}
