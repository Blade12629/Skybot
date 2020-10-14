using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Database.Models.Web;
using AspNet.Security.OAuth.Discord;
using System.Net.Http;
using Skybot.Web.Authentication;

namespace Skybot.Web.Pages
{
    public class LoginModel : PageModel
    {
        public string NotificationMessage { get; set; }

        readonly IHttpClientFactory _clientFactory;

        public LoginModel(IHttpClientFactory clientFactory)
        {
            _clientFactory = clientFactory;
        }

        public async Task<IActionResult> OnGet(string code, string state)
        {
            if (code == null &&
                state == null)
                return Page();
            else if (code.Equals("logout"))
            {
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
                return RedirectToPage("/");
            }
            if (!state.Equals(Request.Host.GetHashCode().ToString(), StringComparison.CurrentCulture))
            {
                NotificationMessage = "Something went wrong while logging in";
                return Page();
            }

            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://discord.com/api/oauth2/token?client_id={SkyBotConfig.DiscordClientId}&client_secret={SkyBotConfig.DiscordClientSecret}&grant_type=authorization_code&code={code}&redirect_uri=http%3A%2F%2Fwindows.dra-gon.wtf%3A40005%2FLogin&scope=identify");
            //HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, $"https://discord.com/api/oauth2/token?client_id={SkyBotConfig.DiscordClientId}&client_secret={SkyBotConfig.DiscordClientSecret}&grant_type=authorization_code&code={code}&redirect_uri=https%3A%2F%2Flocalhost%3A44327%2FLogin&scope=identify");
            _ = request.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string>()
            {
                {"client_id", SkyBotConfig.DiscordClientId },
                {"client_secret", SkyBotConfig.DiscordClientSecret },
                {"grant_type", "authorization_code" },
                {"code", code },
                {"redirect_uri", "http://windows.dra-gon.wtf:40005/Login" },
                //{"redirect_uri", "https://localhost:44327/Login" },
                {"scope", "identify" }
            });

            var client = _clientFactory.CreateClient();
            HttpResponseMessage response = await client.SendAsync(request);
            string json = await response.Content.ReadAsStringAsync();
            OAuth2 oauth = OAuth2.FromString(json);

            request = new HttpRequestMessage(HttpMethod.Get, "https://discordapp.com/api/users/@me");
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {oauth.AccessToken}");

            response = await client.SendAsync(request);
            json = await response.Content.ReadAsStringAsync();

            DSharpPlus.Entities.DiscordUser user = Newtonsoft.Json.JsonConvert.DeserializeObject<DSharpPlus.Entities.DiscordUser>(json);

            if (user == null)
            {
                NotificationMessage = "Unable to get serialize user, please report this bug";
                return Page();
            }

            return await LoginUser((long)user.Id, user.Username);
        }

        public IActionResult OnPost()
        {
            int state = Request.Host.GetHashCode();

            return Redirect($"https://discord.com/api/oauth2/authorize?response_type=code&client_id={SkyBotConfig.DiscordClientId}&scope=identify&state={state}&redirect_uri=http%3A%2F%2Fwindows.dra-gon.wtf%3A40005%2FLogin&prompt=consent");
            //return Redirect($"https://discord.com/api/oauth2/authorize?response_type=code&client_id={SkyBotConfig.DiscordClientId}&scope=identify&state={state}&redirect_uri=https%3A%2F%2Flocalhost%3A44327%2FLogin&prompt=consent");
        }

        async Task<IActionResult> LoginUser(long discordUserId, string username)
        {
            using DBContext c = new DBContext();
            User user = c.User.FirstOrDefault(u => u.DiscordUserId == discordUserId);
            WebUser wuser = c.WebUser.FirstOrDefault(u => u.DiscordUserId == discordUserId);
            
            if (user == null)
            {
                NotificationMessage = "You need to verify first via the bot in order to login";
                return Page();
            }

            Claim cl = CreateLoginClaim(c, user, wuser, username);
            ClaimsIdentity ci = new ClaimsIdentity(new List<Claim>() { cl }, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal cp = new ClaimsPrincipal(ci);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cp).ConfigureAwait(false);
            return RedirectToPage("/CP/ControlPanel");
        }

        Claim CreateLoginClaim(DBContext c, User user, WebUser wuser, string username)
        {
            Claim cl = new Claim(ClaimTypes.NameIdentifier, username);

            if (user.DiscordUserId != 0)
            {
                if (wuser != null)
                    cl.Properties[ClaimProperties.AllowGlobalStats] = wuser.AllowGlobalStats.ToString();
                else
                    cl.Properties[ClaimProperties.AllowGlobalStats] = false.ToString();

                List<Permission> permissions = c.Permission.Where(p => p.DiscordUserId == user.DiscordUserId &&
                                                                       p.AccessLevel >= (short)AccessLevel.Host &&
                                                                       (p.DiscordGuildId != 0 || p.AccessLevel == (short)AccessLevel.Dev))
                                                           .ToList();

                List<Permission> filteredPerms = new List<Permission>();

                for (int i = 0; i < permissions.Count; i++)
                {
                    Permission perm = filteredPerms.FirstOrDefault(p => p.DiscordGuildId == permissions[i].DiscordGuildId);

                    if (perm != null && perm.AccessLevel < permissions[i].AccessLevel)
                    {
                        filteredPerms.Remove(perm);
                        filteredPerms.Add(permissions[i]);
                        continue;
                    }

                    filteredPerms.Add(permissions[i]);
                }

                cl.Properties[ClaimProperties.TotalServers] = filteredPerms.Count.ToString();
                cl.Properties[ClaimProperties.DiscordUserId] = user.DiscordUserId.ToString();

                if (filteredPerms.Count > 0)
                {
                    AccessLevel global = (AccessLevel)filteredPerms[0].AccessLevel;
                    bool useGlobal = global == AccessLevel.Dev;

                    cl.Properties[ClaimProperties.IsDev] = permissions.Any(p => p.DiscordGuildId == 0 && p.AccessLevel == (short)AccessLevel.Dev).ToString();

                    cl.Properties[ClaimProperties.DiscordGuildId] = filteredPerms[0].DiscordGuildId.ToString();
                    cl.Properties[ClaimProperties.AccessLevel] = useGlobal ? global.ToString() : ((AccessLevel)filteredPerms[0].AccessLevel).ToString();

                    for (int i = 1; i < filteredPerms.Count; i++)
                    {
                        cl.Properties[$"{ClaimProperties.DiscordGuildId}{i}"] = permissions[i].DiscordGuildId.ToString();
                        cl.Properties[$"{ClaimProperties.AccessLevel}{i}"] = useGlobal ? global.ToString() : ((AccessLevel)filteredPerms[i].AccessLevel).ToString();
                    }
                }
                else
                {
                    cl.Properties[ClaimProperties.DiscordGuildId] = "0";
                    cl.Properties[ClaimProperties.AccessLevel] = AccessLevel.User.ToString();
                }
            }
            else
            {
                cl.Properties[ClaimProperties.DiscordUserId] = "0";
                cl.Properties[ClaimProperties.DiscordGuildId] = "0";
                cl.Properties[ClaimProperties.AccessLevel] = AccessLevel.User.ToString();
            }

            return cl;
        }
    }
}