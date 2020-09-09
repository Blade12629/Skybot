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
        public bool AllowOverwritingAccessLevel => false;

        public int MinParameters => 1;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            foreach(var guild in client.Guilds)
                guild.Value.Owner.SendMessageAsync(args.ParameterString).Wait();
        }
    }
}
