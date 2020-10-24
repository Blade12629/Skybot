using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class AutoRefSettings
    {
        public ulong DiscordGuildId { get; }
        public int TotalWarmups { get; set; }
        public int BestOf { get; set; }
        public string CaptainBlue { get; set; }
        public string CaptainRed { get; set; }
        public List<string> PlayersBlue { get; set; }
        public List<string> PlayersRed { get; set; }

        public AutoRefSettings(ulong discordGuildId, int totalWarmups, int bestOf, 
                               string captainBlue, string captainRed, List<string> playersBlue, 
                               List<string> playersRed)
        {
            DiscordGuildId = discordGuildId;
            TotalWarmups = totalWarmups;
            BestOf = bestOf;
            CaptainBlue = captainBlue;
            CaptainRed = captainRed;
            PlayersBlue = playersBlue;
            PlayersRed = playersRed;
        }
    }
}
