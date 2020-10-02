using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkyBot.Database.Models.GlobalStatistics;
using System.Linq;
using Skybot.Web.Pages.Api.GlobalStats.Data;

namespace Skybot.Web.Pages.Api.GlobalStats
{
    [ApiController]
    [Route("api/globalstats")]
    public class GlobalStatsController : Controller
    {
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.ApiKeyScheme)]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.AdminScheme)]
        [HttpGet("getprofile/{osuId}")]
        public string GetProfile(long osuId)
        {
            using DBContext dbc = new DBContext();
            PlayerProfile profile = dbc.PlayerProfile.FirstOrDefault(p => p.OsuId == osuId);

            if (profile == null)
                return new ApiResponse(System.Net.HttpStatusCode.NotFound, "Profile not found");

            return new ApiObject<GlobalStatsProfile>(profile);
        }
    }
}
