using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef
{
    //public static class Example
    //{
    //    public static void AutoRefExample()
    //    {
    //        //Load any workflow script
    //        string script = null;

    //        //Get your irc instance
    //        IRC.OsuIrcClient irc = null;

    //        //Setup the builder
    //        AutoRefBuilder builder = new AutoRefBuilder(irc, script)
    //        {
    //            CaptainBlue = "1234",
    //            CaptainRed = "xz"
    //        };

    //        //Create the controller (applies settings and interprets the workflow script to create the workflow)
    //        AutoRefController controller = builder.Build();

    //        //Start the controller and run our workflow
    //        controller.Start(lobbyName: "ABC vs XYZ");
    //    }
    //}

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
                              int totalWarmups, int bestOf, ulong discordGuildId) : this(irc, script, captainBlue, captainRed)
        {
            PlayersBlue = new List<string>(playersBlue);
            PlayersRed = new List<string>(playersRed);
            TotalWarmups = totalWarmups;
            BestOf = bestOf;
            DiscordGuildId = discordGuildId;
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
            arc.Settings = new AutoRefSettings(DiscordGuildId, TotalWarmups, BestOf,
                                               CaptainBlue, CaptainRed, PlayersBlue,
                                               PlayersRed);
        }
    }
}
