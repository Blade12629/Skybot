using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models
{
    public class Mute
    {
        public long Id { get; set; }
        public long DiscordUserId { get; set; }
        public long DiscordGuildId { get; set; }
        public DateTime StartTime { get; set; }
        public long DurationM { get; set; }
        public string Reason { get; set; }
        public bool Unmuted { get; set; }

        public Mute(long discordUserId, long discordGuildId, DateTime startTime, 
                    long durationM, string reason)
        {
            DiscordUserId = discordUserId;
            DiscordGuildId = discordGuildId;
            StartTime = startTime;
            DurationM = durationM;
            Reason = reason;
        }

        public Mute()
        {
        }
    }
}
