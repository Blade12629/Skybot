using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models
{
    public class Reminder
    {
        public long Id { get; set; }
        public long DiscordUserId { get; set; }
        public long DiscordChannelId { get; set; }
        public string Message { get; set; }
        public DateTime EndDate { get; set; }

        public Reminder(long discordUserId, long discordChannelId, string message, DateTime endDate)
        {
            DiscordUserId = discordUserId;
            DiscordChannelId = discordChannelId;
            Message = message;
            EndDate = endDate;
        }

        public Reminder()
        {
        }
    }
}
