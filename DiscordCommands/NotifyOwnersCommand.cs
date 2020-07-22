using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class NotifyOwnersCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "notifyowners";

        public AccessLevel AccessLevel => AccessLevel.Host;

        public CommandType CommandType => CommandType.None;

        public string Description => "Notifies every guild owner where this bot is in";

        public string Usage => "!notifyowners <message>";

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count == 0)
                return;

            StringBuilder message = new StringBuilder(args.Parameters[0]);

            for (int i = 0; i < args.Parameters.Count; i++)
                message.Append(' ' + args.Parameters[i]);

            foreach(var guild in Program.DiscordHandler.Client.Guilds)
                guild.Value.Owner.SendMessageAsync(message.ToString()).Wait();
        }
    }
}
