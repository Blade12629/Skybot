using SkyBot;
using SkyBot.Database.Models.Statistics;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class WarmupCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "warmup";

        public AccessLevel AccessLevel => AccessLevel.Moderator;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Add or remove warmup maps";

        public string Usage => "{prefix}warmup add <beatmapId> [beatmapId] [etc.]\n" +
                               "{prefix}warmup remove <beatmapId> [beatmapId] [etc.]";

        public bool AllowOverwritingAccessLevel => true;

        public int MinParameters => 1;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            using DBContext c = new DBContext();

            Action<long> ac;
            switch(args.Parameters[0].ToLower())
            {
                default:
                case "add":
                    ac = new Action<long>(l =>
                    {
                        WarmupBeatmap wb = c.WarmupBeatmaps.FirstOrDefault(wb => wb.BeatmapId == l);

                        if (wb != null)
                            return;

                        wb = new WarmupBeatmap()
                        {
                            BeatmapId = l,
                            DiscordGuildId = (long)args.Guild.Id
                        };

                        c.WarmupBeatmaps.Add(wb);
                        c.SaveChanges();
                    });
                break;

                case "delete":
                    ac = new Action<long>(l =>
                    {
                        WarmupBeatmap wb = c.WarmupBeatmaps.FirstOrDefault(wb => wb.BeatmapId == l);

                        if (wb == null)
                            return;

                        c.WarmupBeatmaps.Remove(wb);
                        c.SaveChanges();
                    });
                    break;
            }

            foreach (long mapId in GetMaps(args.Parameters))
                ac(mapId);

            args.Channel.SendMessageAsync("Added/Deleted maps");
        }

        private List<long> GetMaps(List<string> parameters, int start = 1)
        {
            List<long> result = new List<long>();

            for (int i = start; i < parameters.Count; i++)
                if (long.TryParse(parameters[i], out long v))
                    result.Add(v);

            return result;
        }
    }
}
