using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Skybot.Web.Pages.Api.Verification.Data;
using SkyBot.Database.Models;

namespace Skybot.Web.Pages.Api.Verification
{
    [ApiController]
    [Route("api/verification")]
    public class VerificationController : Controller
    {
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.ApiKeyScheme)]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.AdminScheme)]
        [HttpGet("getuser/single/{type}/{id}")]
        public string GetUser(string type, ulong id)
        {
            using DBContext dbc = new DBContext();
            User user;
            switch (type.ToLower(CultureInfo.CurrentCulture))
            {
                case "discordid":
                    user = dbc.User.FirstOrDefault(u => u.DiscordUserId == (long)id);
                    break;

                case "osuid":
                    user = dbc.User.FirstOrDefault(u => u.OsuUserId == (long)id);
                    break;

                default:
                    return new ApiResponse(System.Net.HttpStatusCode.BadRequest, "Could not parse type " + type);
            }

            if (user == null)
                return new ApiResponse(System.Net.HttpStatusCode.NotFound, $"User with id {id} not found");

            return new ApiObject<VerifiedUser>(user);
        }

        [Authorize(AuthenticationSchemes = AuthenticationSchemes.ApiKeyScheme)]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.AdminScheme)]
        [HttpGet("getuser/list/")]
        public string GetUsers(int limit = 100, long start = 0)
        {
            using DBContext dbc = new DBContext();
            User[] users = dbc.User.Where(u => u.Id >= start && u.Id < start + limit).ToArray();

            if (users.Length == 0)
                return new ApiResponse(System.Net.HttpStatusCode.NotFound, "No users found");

            return new ApiObject<VerifiedUser[]>(users.Select(u => (VerifiedUser)u).ToArray());
        }
    }
}
