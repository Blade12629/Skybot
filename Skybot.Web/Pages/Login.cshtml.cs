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
            if (string.IsNullOrEmpty(Username) ||
                string.IsNullOrEmpty(Password))
            {
                NotificationMessage = "Password and Username cannot be empty";
                return Page();
            }

            using DBContext c = new DBContext();
            WebUser user = c.WebUser.FirstOrDefault(wu => wu.Username.Equals(Username, StringComparison.CurrentCultureIgnoreCase));

            if (user == null ||
                !VerifyPassword(user.Username, user.PasswordHashed, Password)) 
            {
                NotificationMessage = "Invalid Login Data";
                return Page();
            }

            Claim cl = new Claim(ClaimTypes.NameIdentifier, user.Username);

            if (user.DiscordUserId != 0)
            {
                Permission perms = c.Permission.FirstOrDefault(p => p.DiscordUserId == user.DiscordUserId &&
                                                                    (p.DiscordGuildId != 0 && p.AccessLevel == (short)SkyBot.AccessLevel.Host) ||
                                                                    p.AccessLevel == (short)SkyBot.AccessLevel.Dev);

                cl.Properties["DiscordUserId"] = user.DiscordUserId.ToString();
                cl.Properties["DiscordGuildId"] = perms?.DiscordGuildId.ToString() ?? "0";
                cl.Properties["AccessLevel"] = ((AccessLevel)(perms?.AccessLevel ?? (short)SkyBot.AccessLevel.Host)).ToString();
            }
            else
            {
                cl.Properties["DiscordUserId"] = "0";
                cl.Properties["DiscordGuildId"] = "0";
                cl.Properties["AccessLevel"] = AccessLevel.User.ToString();
            }

            List<Claim> claims = new List<Claim>()
            {
                cl
            };

            ClaimsIdentity ci = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            ClaimsPrincipal cp = new ClaimsPrincipal(ci);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, cp);
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