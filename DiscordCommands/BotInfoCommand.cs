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

        public string Command => "botinfo";

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        public string Description => "Displays general infos about the bot";

        public string Usage => "{prefix}botinfo";

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = "Bot info for Skybot",
                Description = "‎"
            };

            using DBContext c = new DBContext();

            builder.AddField("Guilds", handler.DiscordHandler.Client.Guilds.Count.ToString(CultureInfo.CurrentCulture), true);
            builder.AddField("Users", c.User.Count().ToString(CultureInfo.CurrentCulture), true);
            builder.AddField("Uptime", DateTime.UtcNow.Subtract(Program.StartedOn).ToString());

            args.Channel.SendMessageAsync(embed: builder.Build());
        }
    }
}
