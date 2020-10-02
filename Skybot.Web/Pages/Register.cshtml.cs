using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkyBot.Database.Models.Web;

namespace Skybot.Web.Pages
{
    public class RegisterModel : PageModel
    {
        [BindProperty]
        public string Username { get; set; }
        [BindProperty, DataType(DataType.Password)]
        public string Password { get; set; }
        [BindProperty, DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string NotificationMessage { get; set; }

        public async Task<IActionResult> OnPost()
        {
            if (string.IsNullOrEmpty(Username) ||
                string.IsNullOrEmpty(Password) ||
                string.IsNullOrEmpty(ConfirmPassword))
                return Page();

            if (Username.Length < 4)
            {
                NotificationMessage = "Username requires 4 or more characters";
                return Page();
            }
            else if (Password.Length < 6)
            {
                NotificationMessage = "Password requires 6 or more characters";
                return Page();
            }
            else if (!Password.Equals(ConfirmPassword, StringComparison.CurrentCulture))
            {
                NotificationMessage = "Password does not match";
                return Page();
            }

            using DBContext c = new DBContext();
            WebUser wu = c.WebUser.FirstOrDefault(wu => wu.Username.Equals(Username, StringComparison.CurrentCultureIgnoreCase));

            if (wu != null)
            {
                NotificationMessage = "User already exists";
                return Page();
            }

            c.WebUser.Add(new WebUser(Username, HashPassword(Username, Password)));
            await c.SaveChangesAsync();

            NotificationMessage = "Successfully registered";
            return Page();
        }

        string HashPassword(string user, string pass)
        {
            PasswordHasher<string> hasher = new PasswordHasher<string>();

            return hasher.HashPassword(user, pass);
        }
    }
}