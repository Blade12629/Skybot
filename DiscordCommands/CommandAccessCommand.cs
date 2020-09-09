using SkyBot.Discord.CommandSystem;
using SkyBot;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;
using SkyBot.Discord;
using SkyBot.Database.Models;

namespace DiscordCommands
{
    public class CommandAccessCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "commandaccess";

        public AccessLevel AccessLevel => AccessLevel.Admin;

        public bool AllowOverwritingAccessLevel => false;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Changes/resets the accesslevel of commands (Note, this only changes the accesslevel required to activate commands!)";

        public string Usage => "{prefix}commandaccess <commandName> <new accesslevel(User, VIP, Moderator, Admin, Host)\n{prefix}commandaccess reset <commandName>";

        public int MinParameters => 2;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            if (!args.Parameters[0].Equals("reset", StringComparison.CurrentCultureIgnoreCase))
            {
                AccessLevel newAccess;
                if (args.Parameters[1].TryParseEnum(out AccessLevel acc))
                    newAccess = acc;
                else
                {
                    HelpCommand.ShowHelp(args.Channel, this, string.Format(CultureInfo.CurrentCulture, Resources.FailedParseException, Resources.AccessLevel));
                    return;
                }

                if (newAccess == AccessLevel.Dev)
                {
                    HelpCommand.ShowHelp(args.Channel, this, "Dev can only be set via DB");
                    return;
                }

                if (!handler.SetCommandAccessLevel(args.Parameters[0], args.Guild.Id, newAccess))
                    client.SendSimpleEmbed(args.Channel, "Failed to change accesslevel", "Command not found or command not overwrittable").ConfigureAwait(false);
                else
                    client.SendSimpleEmbed(args.Channel, "Accesslevel changed", "Changed commands accesslevel").ConfigureAwait(false);
            }
            else
            {
                using DBContext c = new DBContext();
                CommandAccess ca = c.CommandAccess.FirstOrDefault(ca => ca.DiscordGuildId == (long)args.Guild.Id &&
                                                                        ca.TypeName.StartsWith(args.Parameters[1], StringComparison.CurrentCultureIgnoreCase));

                if (ca != null)
                {
                    c.CommandAccess.Remove(ca);
                    c.SaveChanges();

                    client.SendSimpleEmbed(args.Channel, "Resetted", "Resetted command " + args.Parameters[1]).ConfigureAwait(false);
                }
                else
                {
                    client.SendSimpleEmbed(args.Channel, "Not Found/Resetted", "Could not find command or command already resetted").ConfigureAwait(false);
                }
            }
        }
    }
}
