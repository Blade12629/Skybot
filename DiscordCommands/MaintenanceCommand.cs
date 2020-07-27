using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class MaintenanceCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => ResourcesCommands.MaintenanceCommand;

        public AccessLevel AccessLevel => AccessLevel.Dev;

        public CommandType CommandType => CommandType.None;

        public string Description => ResourcesCommands.MaintenanceCommandDescription;

        public string Usage => ResourcesCommands.MaintenanceCommandUsage;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count < 2)
            {
                HelpCommand.ShowHelp(args.Channel, this);
                return;
            }
            
            if (!bool.TryParse(args.Parameters[0], out bool status))
            {
                HelpCommand.ShowHelp(args.Channel, this, ResourcesCommands.MaintenanceCommandFailedParseStatus);
                return;
            }

            StringBuilder message = new StringBuilder(args.Parameters[1]);

            for (int i = 2; i < args.Parameters.Count; i++)
                message.Append(' ' + args.Parameters[i]);

            Program.MaintenanceScanner.SetMaintenanceStatus(status, message.ToString());
            args.Channel.SendMessageAsync(string.Format(System.Globalization.CultureInfo.CurrentCulture, ResourcesCommands.MaintenanceCommandSetStatus, status, message.ToString()));
        }
    }
}
