using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class AccessLevelCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "accesslevel";

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        public string Description => "Shows your accesslevel";

        public string Usage => "!accesslevel";

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            args.Channel.SendMessageAsync($"{args.User.Mention} your access level is {Program.DiscordHandler.CommandHandler.GetAccessLevel(args.User, args.Guild)}");
        }
    }
}
