using SkyBot;
using SkyBot.Analyzer;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace DiscordCommands
{
    public class RemoveMatchCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => ResourcesCommands.RemoveMatchCommand;

        public AccessLevel AccessLevel => AccessLevel.Admin;

        public CommandType CommandType => CommandType.Public;

        public string Description => ResourcesCommands.RemoveMatchCommandDescription;

        public string Usage => ResourcesCommands.RemoveMatchCommandUsage;

        public int MinParameters => 1;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (!long.TryParse(args.Parameters[0], out long matchId))
                return;

            OsuAnalyzer.RemoveMatch(matchId, args.Guild);

            args.Channel.SendMessageAsync(ResourcesCommands.RemoveMatchCommandSuccess + matchId + '\n' + ResourceStats.CacheUpdating).ConfigureAwait(false).GetAwaiter().GetResult();

            OsuAnalyzer.UpdateCaches(args.Guild);

            args.Channel.SendMessageAsync(ResourceStats.CacheUpdated);
        }
    }
}
