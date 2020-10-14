using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models
{
    public class AnalyzerMods
    {
        public long Id { get; set; }
        public long DiscordGuildId { get; set; }

        public double NF { get; set; }
        public double EZ { get; set; }
        public double HT { get; set; }
        public double HR { get; set; }
        public double DTNC { get; set; }
        public double HD { get; set; }
        public double FL { get; set; }
        public double RLX { get; set; }
        public double AP { get; set; }

        public AnalyzerMods(long discordGuildId, double nF = 1.0, double eZ = 1.0, double hT = 1.0, double hR = 1.0, double dTNC = 1.0, double hD = 1.0, double fL = 1.0, double rLX = 1.0, double aP = 1.0)
        {
            DiscordGuildId = discordGuildId;
            NF = nF;
            EZ = eZ;
            HT = hT;
            HR = hR;
            DTNC = dTNC;
            HD = hD;
            FL = fL;
            RLX = rLX;
            AP = aP;
        }

        public AnalyzerMods()
        {
        }
    }
}
