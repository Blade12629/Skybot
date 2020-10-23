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
        public string CaptainBlue { get; set; }
        public string CaptainRed { get; set; }
        public List<string> PlayersBlue { get; set; }
        public List<string> PlayersRed { get; set; }

        public IRC.OsuIrcClient IRC { get; set; }

        public string Script { get; set; }

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
                              string captainRed, IEnumerable<string> playersBlue, IEnumerable<string> playersRed) : this(irc, script, captainBlue, captainRed)
        {
            PlayersBlue = new List<string>(playersBlue);
            PlayersRed = new List<string>(playersRed);
        }

        public AutoRefController Build()
        {
            LobbyController lc = new LobbyController(IRC);
            AutoRefController arc = new AutoRefController(lc);

            Workflows.WorkflowEngine engine = new Workflows.WorkflowEngine(lc, arc);
            engine.Interpret(Script);

            arc.AddTicks(engine.Wrapper.GetAllSteps());

            return null;
        }
    }
}
