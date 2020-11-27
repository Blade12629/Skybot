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

        public string Command => "removematch";

        public AccessLevel AccessLevel => AccessLevel.Admin;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Removes a match from the analyzer db";

        public bool AllowOverwritingAccessLevel => true;

        public string Usage => "{prefix}removematch <matchId>";

        public int MinParameters => 1;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            if (!long.TryParse(args.Parameters[0], out long matchId))
                return;

            OsuAnalyzer.RemoveMatch(matchId, args.Guild);

            args.Channel.SendMessageAsync($"Removed match {matchId}\nCache is getting updated, this might take a moment").ConfigureAwait(false).GetAwaiter().GetResult();

            OsuAnalyzer.UpdateCaches(args.Guild);

            args.Channel.SendMessageAsync("Cache was updated");
        }
    }
}
