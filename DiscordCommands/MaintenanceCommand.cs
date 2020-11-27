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

        public string Command => "maintenance";

        public AccessLevel AccessLevel => AccessLevel.Dev;

        public CommandType CommandType => CommandType.None;

        public string Description => "Sets the maintenance message/status";

        public string Usage => "{prefix}maintenance <status> <message>";

        public int MinParameters => 2;
        public bool AllowOverwritingAccessLevel => false;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            if (!bool.TryParse(args.Parameters[0], out bool status))
            {
                HelpCommand.ShowHelp(args.Channel, this, "Failed to parse status");
                return;
            }

            string message = args.ParameterString.Remove(0, args.Parameters[0].Length + 1);

            Program.MaintenanceScanner.SetMaintenanceStatus(status, message);
            args.Channel.SendMessageAsync($"Set status to: {status} and message to: {message}");
        }
    }
}
