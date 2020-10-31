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


        public AutoRefBuilder(IRC.OsuIrcClient irc, string script)
        {
            IRC = irc;
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

        public void TestRun()
        {
            string script = "function HelloWorld()\n{\n\tRef.DebugLog(\"HelloWorld step\");\n\treturn true;\n}\n\nfunction ReturnTrue()\n{\n\tRef.DebugLog(\"ReturnTrue step\");\n\treturn true;\n}\n\nfunction Main()\n{\n\tWorkflow.AddStep(ReturnTrue);\n\tWorkflow.AddStep(HelloWorld);\n}\n\nMain();\nconst fs = require('fs');\nlet data = \"Test Write\";\nfs.writeFile('Output.txt', data, (err) => {\n\tif (err) throw err;\n});";
            LobbyController lc = new LobbyController(IRC);
            AutoRefController arc = new AutoRefController(lc);

            lc.OnException += (s, e) => Logger.Log("LC: " + e, LogLevel.Error);
            arc.OnException += (s, e) => Logger.Log("ARC: " + e, LogLevel.Error);

            Workflows.WorkflowEngine engine = new Workflows.WorkflowEngine(lc, arc);
            Workflows.Wrappers.WorkflowWrapper wrapper = engine.Interpret(script, out Exception ex_);

            if (ex_ != null)
            {
                Logger.Log("Script Error: " + ex_.ToString(), LogLevel.Error);
                return;
            }

            List<Exception> errors = engine.GetErrors();

            if (errors.Count > 0)
            {
                foreach (Exception ex in errors)
                    Logger.Log(ex, LogLevel.Error);

                return;
            }

            arc.AddTicks(wrapper.GetAllSteps());
            arc.TestRun();
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
                    break;
                case 1:
                    if (string.IsNullOrEmpty(arc.Script1))
                        return false;
                    break;
                case 2:
                    if (string.IsNullOrEmpty(arc.Script2))
                        return false;
                    break;
                case 3:
                    if (string.IsNullOrEmpty(arc.Script3))
                        return false;
                    break;
            }

            TotalWarmups = arc.TotalWarmups;
            BestOf = arc.BestOf;
            DiscordGuildId = (ulong)arc.DiscordGuildId;
            DiscordNotifyChannelId = (ulong)arc.DiscordNotifyChannelId;
            PlayersPerTeam = arc.PlayersPerTeam;

            return true;
        }

        public AutoRefController Build(out Exception ex)
        {
            LobbyController lc = new LobbyController(IRC);
            AutoRefController arc = new AutoRefController(lc);

            Apply(arc);

            Workflows.WorkflowEngine engine = new Workflows.WorkflowEngine(lc, arc);
            Workflows.Wrappers.WorkflowWrapper wrapper = engine.Interpret(Script, out Exception ex_);
            
            if (ex_ != null)
            {
                ex = ex_;
                return null;
            }

            arc.AddTicks(wrapper.GetAllSteps());

            ex = null;
            return arc;
        }

        void Apply(AutoRefController arc)
        {
            arc.Settings = new AutoRefSettings(DiscordGuildId, DiscordNotifyChannelId, TotalWarmups, 
                                               BestOf, CaptainBlue, CaptainRed, PlayersBlue,
                                               PlayersRed, PlayersPerTeam);
        }
    }
}
