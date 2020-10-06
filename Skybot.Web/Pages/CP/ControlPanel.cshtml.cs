using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkyBot;

namespace Skybot.Web.Pages.CP
{
    public class ControlPanelModel : PageModel
    {
        public AccessLevel Access { get; set; }
        public long DiscordUserId { get; set; }
        public long DiscordGuildId { get; set; }
        public string Username { get; set; }

        public void OnGet()
        {
            Claim c = User.Claim();

            Access = c.GetAccess();
            DiscordUserId = c.GetDiscordUserId();
            DiscordGuildId = c.GetDiscordGuildId();
            Username = c.Value;
        }
    }
}