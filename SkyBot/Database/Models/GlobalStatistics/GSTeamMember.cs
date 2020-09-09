namespace SkyBot.Database.Models.GlobalStatistics
{
    public class GSTeamMember
    {
        public long Id { get; set; }
        public long GSTeamId { get; set; }
        public long OsuUserId { get; set; }

        public GSTeamMember(long gSTeamId, long osuUserId)
        {
            GSTeamId = gSTeamId;
            OsuUserId = osuUserId;
        }
    }
}
