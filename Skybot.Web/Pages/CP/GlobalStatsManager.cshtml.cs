using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkyBot.Database.Models.GlobalStatistics;

namespace Skybot.Web
{
    public class GlobalStatsManagerModel : PageModel
    {
        [BindProperty]
        public int State { get; set; }

        [BindProperty]
        public int TotalTeams { get; set; }
        [BindProperty]
        public int MaxPlayersPerTeam { get; set; }
        
        [BindProperty]
        public long HostOsuId { get; set; }
        [BindProperty]
        public string TourneyName { get; set; }
        [BindProperty]
        public string TourneyAcronym { get; set; }
        [BindProperty]
        public string TourneyThread { get; set; }
        [BindProperty]
        public string CountryCode { get; set; }
        [BindProperty]
        public DateTime StartDate { get; set; }
        [BindProperty]
        public DateTime EndDate { get; set; }
        [BindProperty]
        public long RankMin { get; set; }
        [BindProperty]
        public long RankMax { get; set; }

        [BindProperty]
        public List<string> TeamNames { get; set; }
        [BindProperty]
        public List<int> Placement { get; set; }
        [BindProperty]
        public List<long> UserIds { get; set; }

        public IActionResult OnGet()
        {
            if (!User.Claim().AllowGlobalStats())
                return RedirectToPage("/CP/ControlPanel");

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            if (!User.Claim().AllowGlobalStats())
                return RedirectToPage("/CP/ControlPanel");

            if (TotalTeams != 0)
            {
                if (HostOsuId != 0)
                {
                    if (UserIds != null && UserIds.Count > 0 &&
                        TeamNames != null && TeamNames.Count == TotalTeams &&
                        Placement != null && Placement.Count == TotalTeams)
                        State = 3;
                    else
                        State = 2;
                }
                else
                    State = 1;
            }

            switch(State)
            {
                default:
                    break;

                case 3:
                    //TeamName, (placement, userIds)
                    Dictionary<string, (int, List<long>)> teams = new Dictionary<string, (int, List<long>)>();

                    for (int i = 0; i < TeamNames.Count; i++)
                    {
                        List<long> userIds = UserIds.GetRange(0, MaxPlayersPerTeam);
                        UserIds.RemoveAll(u => u == 0);
                        UserIds.RemoveRange(0, MaxPlayersPerTeam);

                        int placement = Placement[0];
                        Placement.RemoveAt(0);

                        teams.Add(TeamNames[i], (placement, UserIds));
                    }

                    return await SubmitData(teams);
            }

            return Page();
        }

        async Task<IActionResult> SubmitData(Dictionary<string, (int, List<long>)> teams)
        {
            using DBContext c = new DBContext();

            GSTournament tourney = c.GSTournament.Add(new GSTournament(HostOsuId, TourneyName, TourneyAcronym, TourneyThread, 
                                                    CountryCode, StartDate, EndDate, RankMin, RankMax)).Entity;

            await c.SaveChangesAsync();


            foreach(var pair in teams)
            {
                GSTeam team = c.GSTeam.Add(new GSTeam(tourney.Id, pair.Value.Item1, pair.Key)).Entity;

                await c.SaveChangesAsync();

                foreach(long user in pair.Value.Item2)
                    c.GSTeamMember.Add(new GSTeamMember(team.Id, user));

                await c.SaveChangesAsync();
            }

            return Page();
        }
    }
}