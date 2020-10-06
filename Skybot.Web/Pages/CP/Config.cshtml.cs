using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SkyBot;

namespace Skybot.Web.Pages.CP
{
    public class ConfigModel : PageModel
    {
        [BindProperty]
        public string NotificationMessage { get; set; }
        [BindProperty]
        public long AnalyzeChannelId { get; set; }
        [BindProperty]
        public short AnalyzeWarmupMatches { get; set; }


        [BindProperty]
        public long CommandChannelId { get; set; }

        [BindProperty]
        public bool VerifiedNameAutoSet { get; set; }
        [BindProperty]
        public long VerifiedRoleId { get; set; }

        [BindProperty]
        public long TicketDiscordChannelId { get; set; }

        [BindProperty]
        public string WelcomeMessage { get; set; }
        [BindProperty]
        public long WelcomeChannel { get; set; }

        [BindProperty]
        public long MutedRoleId { get; set; }

        [BindProperty]
        public char? Prefix { get; set; }

        [BindProperty]
        public bool Debug { get; set; }
        [BindProperty]
        public long DebugChannel { get; set; }

        [BindProperty]
        public long BlacklistRoleId { get; set; }

        public void OnGet()
        {
            var claim = User.Claim();

            AccessLevel access = claim.GetAccess();

            if (access < AccessLevel.Host)
                return;

            long guildId = claim.GetDiscordGuildId();

            if (guildId == 0)
                return;

            using DBContext c = new DBContext();
            var cfg = c.DiscordGuildConfig.FirstOrDefault(cfg => cfg.GuildId == guildId);

            if (cfg == null)
                return;

            AnalyzeChannelId = cfg.AnalyzeChannelId;
            AnalyzeWarmupMatches = cfg.AnalyzeWarmupMatches;
            CommandChannelId = cfg.CommandChannelId;
            VerifiedNameAutoSet = cfg.VerifiedNameAutoSet;
            VerifiedRoleId = cfg.VerifiedRoleId;
            TicketDiscordChannelId = cfg.TicketDiscordChannelId;
            WelcomeChannel = cfg.WelcomeChannel;
            WelcomeMessage = cfg.WelcomeMessage;
            MutedRoleId = cfg.MutedRoleId;
            Prefix = cfg.Prefix;
            Debug = cfg.Debug;
            DebugChannel = cfg.DebugChannel;
            BlacklistRoleId = cfg.BlacklistRoleId;
        }

        public async Task<IActionResult> OnPost()
        {
            var claim = User.Claim();

            AccessLevel access = claim.GetAccess();

            if (access < AccessLevel.Host)
                return Page();

            long guildId = claim.GetDiscordGuildId();

            if (guildId == 0)
                return Page();

            using DBContext cnt = new DBContext();
            var c = cnt.DiscordGuildConfig.FirstOrDefault(cfg => cfg.GuildId == guildId);

            if (c == null)
            {
                NotificationMessage = "Failed to get config";
                return Page();
            }

            c.AnalyzeChannelId = AnalyzeChannelId;
            c.AnalyzeWarmupMatches = AnalyzeWarmupMatches;
            c.CommandChannelId = CommandChannelId;
            c.VerifiedNameAutoSet = VerifiedNameAutoSet;
            c.VerifiedRoleId = VerifiedRoleId;
            c.TicketDiscordChannelId = TicketDiscordChannelId;
            c.WelcomeChannel = WelcomeChannel;
            c.WelcomeMessage = WelcomeMessage;
            c.MutedRoleId = MutedRoleId;
            c.Prefix = Prefix;
            c.Debug = Debug;
            c.DebugChannel = DebugChannel;
            c.BlacklistRoleId = BlacklistRoleId;

            cnt.DiscordGuildConfig.Update(c);
            await cnt.SaveChangesAsync();

            NotificationMessage = "Updated config";
            OnGet();
            return Page();
        }
    }
}