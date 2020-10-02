using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Skybot.Web.Pages.Api.Session.Data;
using SkyBot.Database.Models.Statistics;

namespace Skybot.Web.Pages.Api.Session
{
    [Route("api/session")]
    [ApiController]
    public class SessionController : ControllerBase
    {
        [Authorize(AuthenticationSchemes.AdminScheme)]
        [Authorize(AuthenticationSchemes.ApiKeyScheme)]
        [HttpGet("getprofile/{osuUserId}")]
        public string GetProfile(long discordGuildId, long osuUserId)
        {
            using DBContext c = new DBContext();
            SeasonPlayerCardCache cc = c.SeasonPlayerCardCache.FirstOrDefault(p => p.OsuUserId == osuUserId &&
                                                                                   p.DiscordGuildId == discordGuildId);

            if (cc == null)
                return new ApiResponse(System.Net.HttpStatusCode.NotFound, "User not found");

            return new ApiObject<PlayerCard>(cc);
        }

        [Authorize(AuthenticationSchemes.AdminScheme)]
        [Authorize(AuthenticationSchemes.ApiKeyScheme)]
        [HttpGet("getteamprofile/{teamName}")]
        public string GetTeamProfile(long discordGuildId, string teamName)
        {
            if (string.IsNullOrEmpty(teamName))
                return new ApiResponse(System.Net.HttpStatusCode.BadRequest, "Teamname cannot be empty");

            using DBContext c = new DBContext();
            SeasonTeamCardCache cc = c.SeasonTeamCardCache.FirstOrDefault(t => t.TeamName.Equals(teamName, StringComparison.CurrentCultureIgnoreCase) &&
                                                                               t.DiscordGuildId == discordGuildId);

            if (cc == null)
                return new ApiResponse(System.Net.HttpStatusCode.NotFound, "Team not found");

            return new ApiObject<TeamCard>(cc);
        }

        [Authorize(AuthenticationSchemes.AdminScheme)]
        [Authorize(AuthenticationSchemes.ApiKeyScheme)]
        [HttpGet("getresultraw/{matchId}")]
        public string GetResultRaw(long discordGuildId, long matchId)
        {
            using DBContext c = new DBContext();
            SeasonResult result = c.SeasonResult.FirstOrDefault(r => r.MatchId == matchId &&
                                                                     r.DiscordGuildId == discordGuildId);

            if (result == null)
                return new ApiResponse(System.Net.HttpStatusCode.NotFound, "Match not found");

            SessionResult sr = SessionResult.FromResult(result);

            return new ApiObject<SessionResult>(sr);
        }
    }
}