using SkyBot;
using SkyBot.Analyzer;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class ClearMatchesCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "clearmatches";

        public AccessLevel AccessLevel => AccessLevel.Admin;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Clears all matches that are linked to the current server";

        public string Usage => "{prefix}clearmatches";

        public int MinParameters => 0;
        public bool AllowOverwritingAccessLevel => true;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            if (OsuAnalyzer.ClearMatches(args.Guild)) 
            {
                args.Channel.SendMessageAsync($"All matches removed\nCache is getting updated, this might take a moment");
                OsuAnalyzer.UpdateCaches(args.Guild);
                args.Channel.SendMessageAsync("Cache was updated");
            }
            else
                args.Channel.SendMessageAsync("No matches found");
        }
    }
}
