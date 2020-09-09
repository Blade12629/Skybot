namespace SkyBot.Database.Models.GlobalStatistics
{
    public class GSTeam
    {
        public long Id { get; set; }
        /// <summary>
        /// Host osu id
        /// </summary>
        public long GSTournamentId { get; set; }
        /// <summary>
        /// Placement, 1, 2, 3, 4, etc.
        /// </summary>
        public int Placement { get; set; }
        /// <summary>
        /// Team Name
        /// </summary>
        public string Name { get; set; }

        public GSTeam()
        {

        }

        public GSTeam(long gSTournamentId, int placement, string name)
        {
            GSTournamentId = gSTournamentId;
            Placement = placement;
            Name = name;
        }
    }
}
