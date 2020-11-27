using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord;
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

        public string Command => "permission";

        public AccessLevel AccessLevel => AccessLevel.Admin;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Binds or unbinds a permission";

        public string Usage =>  "{prefix}permission bind <discordRoleId> <accessLevel>\n" +
                                "{prefix}permission unbind <discordRoleId> [accessLevel]\n" +
                                "{prefix}permission list [page]";

        public int MinParameters => 1;
        public bool AllowOverwritingAccessLevel => false;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters[0].Equals("list", StringComparison.CurrentCultureIgnoreCase))
            {
                int page = 1;

                if (args.Parameters.Count > 1 && int.TryParse(args.Parameters[1], out int newPage))
                    page = newPage;

                using DBContext c = new DBContext();

                List<DiscordRoleBind> drbs = c.DiscordRoleBind.Where(drb => drb.GuildId == (long)args.Guild.Id).ToList();

                if (drbs.Count == 0)
                {
                    client.SendSimpleEmbed(args.Channel, "Could not find any role binds").ConfigureAwait(false);
                    return;
                }

                StringBuilder roleIdBuilder = new StringBuilder();
                StringBuilder accessBuilder = new StringBuilder();

                for (int i = 0; i < drbs.Count; i++)
                {
                    roleIdBuilder.AppendLine(((ulong)drbs[i].RoleId).ToString(CultureInfo.CurrentCulture));
                    accessBuilder.AppendLine(drbs[i].AccessLevel.ToString(CultureInfo.CurrentCulture));
                }

                EmbedPageBuilder epb = new EmbedPageBuilder(2);
                epb.AddColumn("RoleId", roleIdBuilder.ToString());
                epb.AddColumn("Access Level", accessBuilder.ToString());

                args.Channel.SendMessageAsync(embed: epb.BuildPage(page));
                return;
            }
            else if (args.Parameters.Count < 2)
            {
                HelpCommand.ShowHelp(args.Channel, this, ResourceExceptions.NotEnoughParameters);
                return;
            }

            if (!ulong.TryParse(args.Parameters[1], out ulong roleId))
            {
                HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, ResourceExceptions.FailedParseException, args.Parameters[1]));
                return;
            }
            else if (args.Guild.GetRole(roleId) == null)
            {
                HelpCommand.ShowHelp(args.Channel, this, $"Discord Role {args.Parameters[1]} not found!");
                return;
            }

            AccessLevel? access = null;
            if (args.Parameters.Count > 2)
            {
                if (args.Parameters[2].TryParseEnum(out AccessLevel acc))
                    access = acc;
                else
                {
                    HelpCommand.ShowHelp(args.Channel, this, $"Access Level {args.Parameters[2]} not found!");
                    return;
                }

                if (access.HasValue)
                {
                    switch(access.Value)
                    {
                        case AccessLevel.Dev:
                            args.Channel.SendMessageAsync("You do not have enough permissions");
                            return;

                        case AccessLevel.Admin:
                        case AccessLevel.Host:
                            if (args.AccessLevel < AccessLevel.Host)
                            {
                                args.Channel.SendMessageAsync("You do not have enough permissions");
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
                        HelpCommand.ShowHelp(args.Channel, this, $"Access Level {args.Parameters[2]} not found!");
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
