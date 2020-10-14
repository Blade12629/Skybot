using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Authentication;

namespace Skybot.Web
{
    public class UserSettingsModel : PageModel
    {
        [BindProperty]
        public long SelectedDiscordGuildId { get; set; }

        public SelectList DiscordGuildIds { get; set; }

        public string NotificationMessage { get; set; }

        public void OnGet()
        {
            SelectedDiscordGuildId = Create();
        }

        long Create()
        {
            List<long> servers = User.Claim().GetDiscordGuildIds();
            DiscordGuildIds = new SelectList(servers);

            return servers[0];
        }

        public async Task<IActionResult> OnPost()
        {
            long serverId = Create();

            if (serverId == SelectedDiscordGuildId)
                return Page();

            var claim = User.Claim();
            var serverPair = claim.Properties.FirstOrDefault(c => c.Value.Equals(SelectedDiscordGuildId.ToString(), StringComparison.CurrentCulture));

            if (string.IsNullOrEmpty(serverPair.Key) ||
                string.IsNullOrEmpty(serverPair.Value))
                return OnInternalError();

            claim.Properties[ClaimProperties.DiscordGuildId] = SelectedDiscordGuildId.ToString();
            claim.Properties[serverPair.Key] = serverId.ToString();

            await HttpContext.SignInAsync(User).ConfigureAwait(false);
            NotificationMessage = "Changed server to " + SelectedDiscordGuildId;

            return Page();
        }

        IActionResult OnInternalError()
        {
            NotificationMessage = "Something went wrong changing your server";
            return Page();
        }
    }
}