using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using SkyBot.Database.Models;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Skybot.Web.Pages.Api.Verification
{
    [ApiController]
    [Route("api/verification")]
    public class VerificationController : Controller
    {
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.ApiKeyScheme)]
        [HttpGet("{type}/{id}")]
        public string Get(string type, ulong id)
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

            return JsonConvert.SerializeObject(user);
        }
    }
}
