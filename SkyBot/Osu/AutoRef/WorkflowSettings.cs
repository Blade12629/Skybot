using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class WorkflowSettings
    {
        public object SyncRoot { get; } = new object();

        public TimeSpan MapPickTime { get; }
        public TimeSpan RollTime { get; }
        public TimeSpan ReadyUpTime { get; }
        public DateTime MatchStartTime { get; }

        public TeamMode TeamMode { get; }
        public WinCondition WinCondition { get; }
        public int TotalWarmups { get; }
        


        public int WarmupsPlayed { get; set; }
        public LobbyColor NextPick { get; set; }

        public bool IsMatchPaused { get; set; }
        public string PausedBy { get; set; }
        public LobbyColor PausedByColor { get; set; }

        public int TeamRedWins { get; set; }
        public int TeamBlueWins { get; set; }
        public int PlayCount { get; set; }

        public List<long> PlayedMaps { get; }
        

        public WorkflowSettings(DateTime matchStartTime) : this(TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2),
                                                                matchStartTime, TeamMode.TeamVs, WinCondition.ScoreV2, 2)
        {
            
        }

        public WorkflowSettings(TimeSpan mapPickTime, TimeSpan rollTime, TimeSpan readyUpTime, 
                                DateTime matchStartTime, TeamMode teamMode, WinCondition winCondition, 
                                int totalWarmups)
        {
            MapPickTime = mapPickTime;
            RollTime = rollTime;
            ReadyUpTime = readyUpTime;
            MatchStartTime = matchStartTime;
            TeamMode = teamMode;
            WinCondition = winCondition;
            TotalWarmups = totalWarmups;
        }
    }
}
