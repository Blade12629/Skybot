using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.AutoRef
{
    public class AutoRefConfig
    {
        public long Id { get; set; }
        public string Key { get; set; }
        public long DiscordGuildId { get; set; }
        public long DiscordNotifyChannelId { get; set; }

        public int TeamMode { get; set; }
        public int WinCondition { get; set; }
        public int BestOf { get; set; }
        public int TotalWarmups { get; set; }

        public string Script0 { get; set; }
        public string Script1 { get; set; }
        public string Script2 { get; set; }
        public string Script3 { get; set; }
        public int CurrentScript { get; set; }
        public int PlayersPerTeam { get; set; }

        public AutoRefConfig(string key, long discordGuildId, long discordNotifyChannelId, 
                             int teamMode, int winCondition, int bestOf, int totalWarmups, 
                             string script0, string script1, string script2, string script3, 
                             int currentScript, int playersPerTeam)
        {
            Key = key;
            DiscordGuildId = discordGuildId;
            DiscordNotifyChannelId = discordNotifyChannelId;
            TeamMode = teamMode;
            WinCondition = winCondition;
            BestOf = bestOf;
            TotalWarmups = totalWarmups;
            Script0 = script0;
            Script1 = script1;
            Script2 = script2;
            Script3 = script3;
            CurrentScript = currentScript;
            PlayersPerTeam = playersPerTeam;
        }

        public AutoRefConfig()
        {

        }
    }
}
