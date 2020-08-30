using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class BotInfoCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => ResourcesCommands.BotInfoCommand;

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        public string Description => ResourcesCommands.BotInfoCommandDescription;

        public string Usage => ResourcesCommands.BotInfoCommandUsage;

        public int MinParameters => 0;
        public bool AllowOverwritingAccessLevel => false;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            args.Channel.SendMessageAsync(embed: handler.DiscordHandler.GetBotInfo());
        }
    }
}
