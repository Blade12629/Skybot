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

        public int MinParameters => 2;
        public bool AllowOverwritingAccessLevel => false;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (!bool.TryParse(args.Parameters[0], out bool status))
            {
                HelpCommand.ShowHelp(args.Channel, this, ResourcesCommands.MaintenanceCommandFailedParseStatus);
                return;
            }

            string message = args.ParameterString.Remove(0, args.Parameters[0].Length + 1);

            Program.MaintenanceScanner.SetMaintenanceStatus(status, message);
            args.Channel.SendMessageAsync(string.Format(System.Globalization.CultureInfo.CurrentCulture, 
                                                        ResourcesCommands.MaintenanceCommandSetStatus, status, 
                                                        message));
        }
    }
}
