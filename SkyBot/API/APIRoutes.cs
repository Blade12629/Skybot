using Grapevine.Interfaces.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SkyBot.Database.Models;
using Grapevine.Server;
using System.IO;
using SkyBot.Database.Models.GlobalStatistics;
using SkyBot.API.Network;
using SkyBot.API.Data.GlobalStatistics;
using System.Threading.Tasks;

namespace SkyBot.API
{
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1062 // Validate arguments of public methods
#pragma warning disable CA1303 // Do not pass literals as localized parameters
    [RestResource]
    public class APIRoutes
    {
        /// <summary>
        /// Checks for the api key, if invalid sends a respond and returns false
        /// </summary>
        public static bool CheckApiKey(IHttpContext c)
        {
            if (!APIAuth.CheckApiKey(c.Request.Headers.Get("apikey")) &&
                !APIAuth.CheckApiKey(c.Request.QueryString["apikey"]))
            {
                Respond(HttpStatusCode.Unauthorized, Resources.APIInvalidKey, c);
                return false;
            }

            return true;
        }

        public static AccessLevel GetApiKeyAccess(IHttpContext c)
        {
            AccessLevel access = APIAuth.GetApiKeyAccess(c.Request.Headers.Get("apikey"));
            AccessLevel secAccess = APIAuth.GetApiKeyAccess(c.Request.QueryString["apikey"]);

            if (secAccess > access)
                access = secAccess;

            return access;
        }

        /// <summary>
        /// Sends a response
        /// </summary>
        public static void Respond(HttpStatusCode code, string message, IHttpContext context, ContentType? content = null)
        {
            if (content.HasValue)
                context.Response.ContentType = content.Value;

            context.Response.SendResponse(code, message);
        }

        /// <summary>
        /// Sends any object converted to a json representation
        /// </summary>
        public static void RespondAsJson(HttpStatusCode code, object jsonObject, IHttpContext context)
        {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(jsonObject, Newtonsoft.Json.Formatting.Indented);

            Respond(code, json, context, ContentType.JSON);
        }

        /// <summary>
        /// Check if any request is legal
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        [RestRoute]
        public IHttpContext BaseRoute(IHttpContext c)
        {
            if (!APIListener.Listener.Server.Router.RoutingTable.Any(r => r.PathInfo.Equals(c.Request.PathInfo, StringComparison.CurrentCultureIgnoreCase)))
                Respond(HttpStatusCode.NotFound, $"Endpoint '{c.Request.PathInfo}' does not exist!", c);

            return c;
        }

        [RestResource(BasePath = "/verification")]
        public class Verification
        {
            /// <summary>
            /// Gets a specific verified user, Parameters: type: string (discordid/osuid), id: ulong/long (discord: ulong, osu: long)
            /// </summary>
            [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/getuser")]
            public IHttpContext GetUser(IHttpContext c)
            {
                if (!CheckApiKey(c))
                    return c;

                string type = c.Request.QueryString["type"];
                string id = c.Request.QueryString["id"];

                if (string.IsNullOrEmpty(type))
                    type = "discordid";
                else
                    type = type.ToLower(CultureInfo.CurrentCulture);

                using DBContext dbc = new DBContext();

                User user;
                switch(type)
                {
                    default:
                    case "discordid":
                        if (!ulong.TryParse(id, out ulong dId))
                        {
                            Respond(HttpStatusCode.NotFound, $"Unable to parse discord id {id}", c);
                            return c;
                        }

                        user = dbc.User.FirstOrDefault(u => u.DiscordUserId == (long)dId);
                        break;

                    case "osuid":
                        if (!ulong.TryParse(id, out ulong oId))
                        {
                            Respond(HttpStatusCode.NotFound, $"Unable to parse osu id {id}", c);
                            return c;
                        }

                        user = dbc.User.FirstOrDefault(u => u.OsuUserId == (long)oId);
                        break;
                }

                if (user == null)
                {
                    Respond(HttpStatusCode.NotFound, "User not found", c);
                    return c;
                }

                RespondAsJson(HttpStatusCode.Ok, user, c);
                return c;
            }

            /// <summary>
            /// Gets a list of verified users, Parameters: limit: int (max 100), start: long
            /// </summary>
            [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/listusers")]
            public IHttpContext ListUsers(IHttpContext c)
            {
                if (!CheckApiKey(c))
                    return c;

                int limit = 100;
                long start = 0;

                if (!string.IsNullOrEmpty(c.Request.QueryString["limit"]) &&
                    int.TryParse(c.Request.QueryString["limit"], out int newLimit))
                    limit = Math.Max(1, Math.Min(newLimit, limit));

                if (!string.IsNullOrEmpty(c.Request.QueryString["start"]) &&
                    long.TryParse(c.Request.QueryString["start"], out long newStart))
                    start = Math.Max(0, newStart);

                using DBContext dbc = new DBContext();
                User[] users = dbc.User.Where(u => u.Id >= start && u.Id < start + limit).ToArray();

                if (users.Length == 0)
                {
                    Respond(HttpStatusCode.NotFound, "No users found", c);
                }
                else
                {
                    RespondAsJson(HttpStatusCode.Ok, users, c);
                }

                return c;
            }
        }

        [RestResource(BasePath = "/globalstats")]
        public class GlobalStats
        {
            [RestRoute(HttpMethod = HttpMethod.POST, PathInfo = "/submit")]
            public IHttpContext Submit(IHttpContext c)
            {
                try
                {
                    if (!CheckApiKey(c))
                        return c;
                    else if (GetApiKeyAccess(c) <= AccessLevel.Host)
                    {
                        Respond(HttpStatusCode.Forbidden, "Not Enough permissions", c);
                        return c;
                    }

                    List<long> userIds = new List<long>();

                    BinaryAPIReader reader = new BinaryAPIReader(((HttpRequest)c.Request).Advanced.InputStream);
                    GlobalStatsTournament tourneystats = new GlobalStatsTournament();
                    tourneystats.Deserialize(reader);

                    using DBContext dbc = new DBContext();

                    GSTournament tourney = dbc.GSTournament.Add(new GSTournament(tourneystats.HostOsuId, tourneystats.Name, tourneystats.Acronym, tourneystats.Thread, tourneystats.CountryCode, tourneystats.Start, tourneystats.End, tourneystats.RankMin, tourneystats.RankMax)).Entity;
                    dbc.SaveChanges();

                    foreach (var teamstats in tourneystats.Teams)
                    {
                        GSTeam team = dbc.GSTeam.Add(new GSTeam(tourney.Id, teamstats.Placement, teamstats.Name)).Entity;
                        dbc.SaveChanges();

                        foreach (long userId in teamstats.OsuUserIds)
                        {
                            GSTeamMember member = new GSTeamMember(team.Id, userId);
                            dbc.GSTeamMember.Add(member);
                            userIds.Add(userId);
                        }
                    }

                    dbc.SaveChanges();

                    Task.Run(() => SkyBot.GlobalStatistics.GSStatisticHandler.UpdatePlayerProfiles(tourney.Id)).ConfigureAwait(false);

                    Respond(HttpStatusCode.Ok, "Submitted and updated", c);
                    return c;

                }
                catch (Exception ex)
                {
                    Logger.Log(ex, LogLevel.Error);
                    Respond(HttpStatusCode.InternalServerError, ex.ToString(), c);
                    return c;
                }
            }

            /// <summary>
            /// Gets a specific player profile, Parameters: id: int (osuid)
            /// </summary>
            [RestRoute(HttpMethod = HttpMethod.GET, PathInfo = "/getprofile")]
            public IHttpContext GetProfile(IHttpContext c)
            {
                if (!CheckApiKey(c))
                    return c;

                string osuIdStr = c.Request.QueryString["id"];

                if (string.IsNullOrEmpty(osuIdStr))
                {
                    Respond(HttpStatusCode.NotFound, "id not found", c);
                    return c;
                }

                if (!long.TryParse(osuIdStr, out long osuId))
                {
                    Respond(HttpStatusCode.InternalServerError, "Failed to parse id", c);
                    return c;
                }

                using DBContext dbc = new DBContext();
                PlayerProfile profile = dbc.PlayerProfile.FirstOrDefault(p => p.OsuId == osuId);

                if (profile == null)
                {
                    Respond(HttpStatusCode.NotFound, "Profile not found", c);
                    return c;
                }

                BinaryAPIWriter writer = new BinaryAPIWriter(c);
                GlobalStatsProfile gsprofile = (GlobalStatsProfile)profile;

                gsprofile.Serialize(writer);

                Respond(HttpStatusCode.Ok, "Sent", c);
                return c;
            }
        }
    }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
#pragma warning restore CA1062 // Validate arguments of public methods
#pragma warning restore CA1822 // Mark members as static
}
