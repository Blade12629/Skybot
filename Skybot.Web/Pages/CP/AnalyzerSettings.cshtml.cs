using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Skybot.Web
{
    public class AnalyzerSettingsModel : PageModel
    {
        [BindProperty]
        public long AnalyzeChannelId { get; set; }
        [BindProperty]
        public short AnalyzeWarmupMatches { get; set; }

        [BindProperty]
        public double NF { get; set; }
        [BindProperty]
        public double EZ { get; set; }
        [BindProperty]
        public double HR { get; set; }
        [BindProperty]
        public double DTNC { get; set; }
        [BindProperty]
        public double HD { get; set; }
        [BindProperty]
        public double FL { get; set; }

        public string NotificationMessage { get; set; }

        public void OnGet()
        {
            using DBContext c = new DBContext();
            long guildId = User.Claim().GetDiscordGuildId();
            var dgc = c.DiscordGuildConfig.FirstOrDefault(cfg => cfg.GuildId == guildId);
            var mods = c.AnalyzerMods.FirstOrDefault(cfg => cfg.DiscordGuildId == guildId);

            if (dgc == null)
            {
                NotificationMessage = "Failed to find configuration";
                return;
            }
            else if (mods == null)
            {
                mods = c.AnalyzerMods.Add(new SkyBot.Database.Models.AnalyzerMods(guildId)).Entity;
                c.SaveChanges();
            }

            AnalyzeChannelId = dgc.AnalyzeChannelId;
            AnalyzeWarmupMatches = dgc.AnalyzeWarmupMatches;
            NF = mods.NF;
            EZ = mods.EZ;
            HR = mods.HR;
            DTNC = mods.DTNC;
            HD = mods.HD;
            FL = mods.FL;
        }

        public async Task<IActionResult> OnPost()
        {
            using DBContext c = new DBContext();
            long guildId = User.Claim().GetDiscordGuildId();
            var dgc = c.DiscordGuildConfig.FirstOrDefault(cfg => cfg.GuildId == guildId);
            var mods = c.AnalyzerMods.FirstOrDefault(cfg => cfg.DiscordGuildId == guildId);

            if (dgc == null)
            {
                NotificationMessage = "Failed to find configuration";
                return Page();
            }
            else if (mods == null)
            {
                mods = c.AnalyzerMods.Add(new SkyBot.Database.Models.AnalyzerMods(guildId)).Entity;
                await c.SaveChangesAsync();
            }

            dgc.AnalyzeChannelId = AnalyzeChannelId;
            dgc.AnalyzeWarmupMatches = AnalyzeWarmupMatches;
            mods.NF = NF;
            mods.EZ = EZ;
            mods.HR = HR;
            mods.DTNC = DTNC;
            mods.HD = HD;
            mods.FL = FL;

            c.DiscordGuildConfig.Update(dgc);
            c.AnalyzerMods.Update(mods);
            await c.SaveChangesAsync();

            NotificationMessage = "Updated Analyzer Settings";

            return Page();
        }
    }
}