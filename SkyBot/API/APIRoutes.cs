﻿using Grapevine.Interfaces.Server;
using Grapevine.Server.Attributes;
using Grapevine.Shared;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using SkyBot.Database.Models;
using Grapevine.Server;

namespace SkyBot.API
{
#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1062 // Validate arguments of public methods
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
    }
#pragma warning restore CA1062 // Validate arguments of public methods
#pragma warning restore CA1822 // Mark members as static
}
