using System;

namespace SkyBot.Database.Models
{
    public class Ticket
    {
        public long Id { get; set; }
        public long DiscordId { get; set; }
        public long DiscordGuildId { get; set; }
        public short Tag { get; set; }
        public short Status { get; set; }
        public short Priority { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }

        public Ticket(long discordId, long discordGuildId, short tag, short status,
                         short priority, DateTime timestamp, string message)
        {
            DiscordGuildId = discordGuildId;
            DiscordId = discordId;
            Tag = tag;
            Status = status;
            Priority = priority;
            Timestamp = timestamp;
            Message = message;
        }

        public Ticket()
        {
        }
    }
}
