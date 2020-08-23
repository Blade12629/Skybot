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

        public string Usage => "!clearmatches";

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Guild == null)
            {
                HelpCommand.ShowHelp(args.Channel, this);
                return;
            }

            using (DBContext c = new DBContext())
            {
                var matches = c.SeasonResult.Where(sr => sr.DiscordGuildId == (long)args.Guild.Id).ToList();

                if (matches.Count == 0)
                {
                    args.Channel.SendMessageAsync("No matches found");
                    return;
                }

                for (int i = 0; i < matches.Count; i++)
                    OsuAnalyzer.RemoveMatch(matches[i], c);

                c.SaveChanges();

                args.Channel.SendMessageAsync("All matches removed\n" + ResourceStats.CacheUpdating);
            }

            OsuAnalyzer.UpdateCaches(args.Guild);

            args.Channel.SendMessageAsync(ResourceStats.CacheUpdated);
        }
    }
}
