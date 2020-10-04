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
using SkyBot.Database.Models;
using SkyBot.Database.Models.Web;

namespace Skybot.Web.Pages
{
    public class LoginModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }
        [BindProperty, DataType(DataType.Password)]
        public string Password { get; set; }

        public string NotificationMessage { get; set; }

        
        public async Task<IActionResult> OnPost()
        {
            using DBContext c = new DBContext();
            WebUser user = c.WebUser.FirstOrDefault(wu => wu.Username.Equals(Username, StringComparison.CurrentCultureIgnoreCase));

            if (user == null ||
                !VerifyPassword(user.Username, user.PasswordHashed, Password)) 
            {
                NotificationMessage = "Invalid Login Data";
                return Page();
            }

            Claim cl = new Claim(ClaimTypes.Name, user.Username);
            cl.Properties["Username"] = user.Username;

            if (user.DiscordUserId != 0)
            {
                Permission perms = c.Permission.FirstOrDefault(p => p.DiscordUserId == user.DiscordUserId &&
                                                                    (p.DiscordGuildId != 0 && p.AccessLevel == (short)SkyBot.AccessLevel.Host) ||
                                                                    p.AccessLevel == (short)SkyBot.AccessLevel.Dev);
                cl.Properties["DiscordUserId"] = user.DiscordUserId.ToString();
                cl.Properties["DiscordGuildId"] = perms?.DiscordGuildId.ToString() ?? "0";
                cl.Properties["AccessLevel"] = ((SkyBot.AccessLevel)(perms?.AccessLevel ?? (short)SkyBot.AccessLevel.Host)).ToString();
            }
            else
            {
                cl.Properties["DiscordUserId"] = "0";
                cl.Properties["DiscordGuildId"] = "0";
                cl.Properties["AccessLevel"] = SkyBot.AccessLevel.User.ToString();
            }

            List<Claim> claims = new List<Claim>()
            {
                cl
            };
            ClaimsIdentity identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));
            return RedirectToPage("/CP/ControlPanel");
        }

        bool VerifyPassword(string user, string hashedPass, string userInput)
        {
            PasswordHasher<string> hasher = new PasswordHasher<string>();
            PasswordVerificationResult result = hasher.VerifyHashedPassword(user, hashedPass, userInput);

            return result == PasswordVerificationResult.Success;
        }
    }
}