using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Skybot.Web.Pages.CP
{
    public class ConfigModel : PageModel
    {
        public long AnalyzeChannelId { get; set; }
        public short AnalyzeWarmupMatches { get; set; }


        public long CommandChannelId { get; set; }

        public bool VerifiedNameAutoSet { get; set; }
        public long VerifiedRoleId { get; set; }

        public long TicketDiscordChannelId { get; set; }

        public string WelcomeMessage { get; set; }
        public long WelcomeChannel { get; set; }

        public long MutedRoleId { get; set; }

        public char? Prefix { get; set; }

        public bool Debug { get; set; }
        public long DebugChannel { get; set; }

        public long BlacklistRoleId { get; set; }

        public void OnGet()
        {
        }
    }
}