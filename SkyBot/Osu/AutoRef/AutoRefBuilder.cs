using System;
using System.Collections.Generic;
using System.Text;
using SkyBot.Database.Models.AutoRef;
using System.Linq;

namespace SkyBot.Osu.AutoRef
{
    public class AutoRefBuilder
    {
        public IRC.OsuIrcClient IRC { get; set; }

        public string CaptainBlue { get; set; }
        public string CaptainRed { get; set; }
        public List<string> PlayersBlue { get; set; }
        public List<string> PlayersRed { get; set; }
        public string Script { get; set; }


        public int TotalWarmups { get; set; }
        public int BestOf { get; set; }
        public ulong DiscordGuildId { get; set; }
        public ulong DiscordNotifyChannelId { get; set; }
        public int PlayersPerTeam { get; set; }

        public AutoRefBuilder(IRC.OsuIrcClient irc)
        {
            IRC = irc;
        }

        public AutoRefBuilder(IRC.OsuIrcClient irc, string script) : this(irc)
        {
            Script = script;
        }

        public AutoRefBuilder(IRC.OsuIrcClient irc, string script, string captainBlue, 
                              string captainRed) : this(irc, script)
        {
            CaptainBlue = captainBlue;
            CaptainRed = captainRed;
        }

        public AutoRefBuilder(IRC.OsuIrcClient irc, string script, string captainBlue, 
                              string captainRed, IEnumerable<string> playersBlue, IEnumerable<string> playersRed,
                              int totalWarmups, int bestOf, ulong discordGuildId, int playersPerTeam, ulong discordNotifyChannelId) : this(irc, script, captainBlue, captainRed)
        {
            PlayersBlue = new List<string>(playersBlue);
            PlayersRed = new List<string>(playersRed);
            TotalWarmups = totalWarmups;
            BestOf = bestOf;
            DiscordGuildId = discordGuildId;
            PlayersPerTeam = playersPerTeam;
            DiscordNotifyChannelId = discordNotifyChannelId;
        }

        public bool LoadByKeyAndId(string key, ulong discordGuildId)
        {
            using DBContext c = new DBContext();
            AutoRefConfig arc = c.AutoRefConfig.FirstOrDefault(cfg => cfg.DiscordGuildId == (long)discordGuildId && cfg.Key.Equals(key, StringComparison.CurrentCultureIgnoreCase));

            if (arc == null)
                return false;

            switch(arc.CurrentScript)
            {
                default:
                    return false;

                case 0:
                    if (string.IsNullOrEmpty(arc.Script0))
                        return false;

                    Script = arc.Script0;
                    break;
                case 1:
                    if (string.IsNullOrEmpty(arc.Script1))
                        return false;

                    Script = arc.Script1;
                    break;
                case 2:
                    if (string.IsNullOrEmpty(arc.Script2))
                        return false;

                    Script = arc.Script2;
                    break;
                case 3:
                    if (string.IsNullOrEmpty(arc.Script3))
                        return false;

                    Script = arc.Script3;
                    break;
            }

            TotalWarmups = arc.TotalWarmups;
            BestOf = arc.BestOf;
            DiscordGuildId = (ulong)arc.DiscordGuildId;
            DiscordNotifyChannelId = (ulong)arc.DiscordNotifyChannelId;
            PlayersPerTeam = arc.PlayersPerTeam;

            return true;
        }

        public void Apply(AutoRefController arc)
        {
            arc.Settings = new AutoRefSettings(DiscordGuildId, DiscordNotifyChannelId, TotalWarmups, 
                                               BestOf, CaptainBlue, CaptainRed, PlayersBlue,
                                               PlayersRed, PlayersPerTeam);
        }
    }
}
