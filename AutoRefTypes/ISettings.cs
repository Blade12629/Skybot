using System;
using System.Collections.Generic;
using System.Text;

namespace AutoRefTypes
{
    public interface ILobbySettings
    {
        public long MatchId { get; }
        public string ChannelName { get; }
        public string RoomName { get; }
        public string HistoryUrl { get; }
        public long CurrentBeatmapId { get; }
        public TeamMode TeamMode { get; }
        public WinCondition WinCondition { get; }
        public long Mods { get; }
    }
}
