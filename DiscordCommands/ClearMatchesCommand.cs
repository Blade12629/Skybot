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

        public string Command => ResourcesCommands.ClearMatchesCommand;

        public AccessLevel AccessLevel => AccessLevel.Admin;

        public CommandType CommandType => CommandType.Public;

        public string Description => ResourcesCommands.ClearMatchesCommandDescription;

        public string Usage => ResourcesCommands.ClearMatchesCommandUsage;

        public int MinParameters => 0;
        public bool AllowOverwritingAccessLevel => true;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            if (OsuAnalyzer.ClearMatches(args.Guild)) 
            {
                args.Channel.SendMessageAsync($"{ResourcesCommands.ClearMatchesCommandMatchesRemoved}\n" + ResourceStats.CacheUpdating);
                OsuAnalyzer.UpdateCaches(args.Guild);
                args.Channel.SendMessageAsync(ResourceStats.CacheUpdated);
            }
            else
                args.Channel.SendMessageAsync(ResourcesCommands.ClearMatchesCommandMatchesNotFound);
        }
    }
}
