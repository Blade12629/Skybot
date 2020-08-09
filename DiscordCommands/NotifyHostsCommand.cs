using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class NotifyHostsCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "notifyhosts";

        public AccessLevel AccessLevel => AccessLevel.Dev;

        public CommandType CommandType => CommandType.None;

        public string Description => "Notifies every host";

        public string Usage => "!notifyhosts <message>";

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count == 0)
                return;

            foreach(var guild in Program.DiscordHandler.Client.Guilds)
                guild.Value.Owner.SendMessageAsync(args.ParameterString).Wait();
        }
    }
}
