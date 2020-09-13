using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    public class Lobby
    {
        public long Id { get; set; }

        public long MatchId { get; set; }
        public string Host { get; set; }
        public string Channel { get; set; }
        public string MatchName { get; set; }

        public Lobby(long matchId, string host, string channel, string matchName)
        {
            MatchId = matchId;
            Host = host;
            Channel = channel;
            MatchName = matchName;
        }

        public Lobby()
        {
        }
    }
}
