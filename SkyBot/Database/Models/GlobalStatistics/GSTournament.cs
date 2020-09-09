using System;

namespace SkyBot.Database.Models.GlobalStatistics
{
    public class GSTournament
    {
        public long Id { get; set; }
        /// <summary>
        /// Host osu id
        /// </summary>
        public long HostOsuId { get; set; }
        /// <summary>
        /// Tourney name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Tourney name acronym
        /// </summary>
        public string Acronym { get; set; }
        /// <summary>
        /// Tourney thread url
        /// </summary>
        public string Thread { get; set; }
        /// <summary>
        /// Tourney country code, do "0" if not limited
        /// </summary>
        public string CountryCode { get; set; }
        /// <summary>
        /// Tourney Start
        /// </summary>
        public DateTime Start { get; set; }
        /// <summary>
        /// Tourney End
        /// </summary>
        public DateTime End { get; set; }

        public long RankMin { get; set; }
        public long RankMax { get; set; }

        public GSTournament()
        {

        }

        public GSTournament(long hostOsuId, string name, string acronym, string thread, string countryCode, DateTime start, DateTime end, long rankMin, long rankMax)
        {
            HostOsuId = hostOsuId;
            Name = name;
            Acronym = acronym;
            Thread = thread;
            CountryCode = countryCode;
            Start = start;
            End = end;
            RankMin = rankMin;
            RankMax = rankMax;
        }
    }
}
