using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class Settings
    {
        public long MatchId { get; set; }
        public string ChannelName => $"#mp_{MatchId}";
        public string RoomName { get; set; }
        public string HistoryUrl { get; set; }
        public long CurrentBeatmapId { get; set; }
        public TeamMode TeamMode { get; set; }
        public WinCondition WinCondition { get; set; }
        public long Mods { get; set; }

        public Settings(long matchId, string roomName, string historyUrl, 
                            long currentBeatmapId, TeamMode teamMode, WinCondition winCondition, 
                            long mods)
        {
            MatchId = matchId;
            RoomName = roomName;
            HistoryUrl = historyUrl;
            CurrentBeatmapId = currentBeatmapId;
            TeamMode = teamMode;
            WinCondition = winCondition;
            Mods = mods;
        }

        public Settings()
        {
        }

        public Settings Copy()
        {
            return new Settings(MatchId, RoomName, HistoryUrl, CurrentBeatmapId,
                                    TeamMode, WinCondition, Mods);
        }

        public void Reset()
        {
            MatchId = 0;
            RoomName = null;
            HistoryUrl = null;
            CurrentBeatmapId = 0;
            TeamMode = TeamMode.HeadToHead;
            WinCondition = WinCondition.Score;
            Mods = 0;
        }
    }
}
