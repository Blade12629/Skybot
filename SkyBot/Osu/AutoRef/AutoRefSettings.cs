using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class AutoRefSettings
    {
        public ulong DiscordGuildId { get; }
        public ulong DiscordNotifyChannelId { get; set; }
        public int TotalWarmups { get; set; }
        public int BestOf { get; set; }
        public string CaptainBlue { get; set; }
        public string CaptainRed { get; set; }
        public List<string> PlayersBlue { get; set; }
        public List<string> PlayersRed { get; set; }
        public int PlayersPerTeam { get; set; }

        public AutoRefSettings(ulong discordGuildId, ulong discordNotifyChannelId, int totalWarmups, int bestOf, 
                               string captainBlue, string captainRed, List<string> playersBlue, 
                               List<string> playersRed, int playersPerTeam)
        {
            DiscordGuildId = discordGuildId;
            DiscordNotifyChannelId = discordNotifyChannelId;
            TotalWarmups = totalWarmups;
            BestOf = bestOf;
            CaptainBlue = captainBlue;
            CaptainRed = captainRed;
            PlayersBlue = playersBlue;
            PlayersRed = playersRed;
            PlayersPerTeam = playersPerTeam;
        }
    }
}
