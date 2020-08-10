using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SkyBot.Database.Models
{
    public class DiscordGuildConfig
    {
        public long Id { get; set; }
        public long GuildId { get; set; }

        public long AnalyzeChannelId { get; set; }
        public short AnalyzeWarmupMatches { get; set; }


        public long CommandChannelId { get; set; }

        public bool VerifiedNameAutoSet { get; set; }
        public long VerifiedRoleId { get; set; }

        public long TicketDiscordChannelId { get; set; }

        public string WelcomeMessage { get; set; }
        public long WelcomeChannel { get; set; }

        public long MutedRoleId { get; set; }

        public DiscordGuildConfig(long guildId, long analyzeChannelId, long commandChannelId, 
                                  bool verifiedNameAutoSet, long verifiedRoleId, short analyzeWarmupMatches,
                                  long ticketDiscordChannelId, string welcomeMessage, long welcomeChannel,
                                  long mutedRoleId)
        {
            GuildId = guildId;
            AnalyzeChannelId = analyzeChannelId;
            CommandChannelId = commandChannelId;
            VerifiedNameAutoSet = verifiedNameAutoSet;
            VerifiedRoleId = verifiedRoleId;
            AnalyzeWarmupMatches = analyzeWarmupMatches;
            TicketDiscordChannelId = ticketDiscordChannelId;
            WelcomeMessage = welcomeMessage;
            WelcomeChannel = welcomeChannel;
            MutedRoleId = mutedRoleId;
        }

        public DiscordGuildConfig()
        {
        }

        public bool TrySetValue(string key, string value)
        {
            try
            {
                if (string.IsNullOrEmpty(value) || string.IsNullOrEmpty(key))
                    return false;

                PropertyInfo prop = typeof(DiscordGuildConfig).GetProperties().FirstOrDefault(pr => pr.Name.Equals(key, StringComparison.CurrentCulture));
                prop.SetValue(this, Convert.ChangeType(value, prop.PropertyType, System.Globalization.CultureInfo.CurrentCulture));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public string TryGetValue(string key)
        {
            try
            {
                PropertyInfo prop = typeof(DiscordGuildConfig).GetProperties().FirstOrDefault(pr => pr.Name.Equals(key, StringComparison.CurrentCulture));

                return prop.GetValue(this)?.ToString() ?? "Not found";
            }
            catch (Exception)
            {
                return "Not found";
            }
        }

        public static List<string> GetKeys()
        {
            return typeof(DiscordGuildConfig).GetProperties().Select(s => s.Name).ToList();
        }
    }
}
