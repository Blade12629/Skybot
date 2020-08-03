using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.TicketSystem
{
    [Flags]
    public enum TicketTag
    {
        None = 0,

        Commentator = 1,
        Streamer = 2,
        MappoolSelector = 4,
        Referee = 8,
        Developer = 16,
        Organizer = 32
    }
}
