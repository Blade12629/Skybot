using SkyBot;
using SkyBot.Osu.AutoRef;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Globalization;

namespace DiscordCommands
{
    public class RefCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "ref";

        public AccessLevel AccessLevel => AccessLevel.Dev;

        public CommandType CommandType => CommandType.Public;

        public string Description => "DEBUG TOOL";

        public string Usage => "No Description Available";

        public bool AllowOverwritingAccessLevel => false;

        public int MinParameters => 0;

        static AutoRefController _arc;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            string @param = args.Parameters[0].ToLower(CultureInfo.CurrentCulture);
            args.ParameterString = args.ParameterString.Remove(0, args.Parameters[0].Length + 1);
            args.Parameters.RemoveAt(0);

            switch (@param)
            {
                default:
                    args.Channel.SendMessageAsync("Unkown command").ConfigureAwait(false);
                    return;

                case "-script":
                    {
                        string script = null;
                        using (System.Net.WebClient wc = new System.Net.WebClient())
                        {
                            script = wc.DownloadString(args.ParameterString);
                        }

                        if (string.IsNullOrEmpty(script))
                        {
                            args.Channel.SendMessageAsync("Unable to get script").ConfigureAwait(false);
                            return;
                        }

                        using DBContext c = new DBContext();
                        var arc = c.AutoRefConfig.First(arc => arc.DiscordGuildId == (long)arc.DiscordGuildId && arc.Key.Equals("test", StringComparison.CurrentCultureIgnoreCase));
                        arc.Script0 = script;

                        c.AutoRefConfig.Update(arc);
                        c.SaveChanges();

                        args.Channel.SendMessageAsync("Updated script").ConfigureAwait(false);
                    }
                    return;

                case "-test":
                    {
                        string capBlue = args.Parameters[0];
                        string capRed = args.Parameters[1];

                        TestMatch(capBlue, capRed);
                    }
                    return;

                case "-stop":
                    {
                        _arc.LC.EnqueueCloseLobby();
                    }
                    return;
            }
        }
        
        void TestMatch(string capBlue, string capRed)
        {
            AutoRefBuilder arb = new AutoRefBuilder(Program.IRC)
            {
                CaptainBlue = capBlue,
                CaptainRed = capRed,
                PlayersBlue = new List<string>(),
                PlayersRed = new List<string>()
            };
            arb.LoadByKeyAndId("test", 738155828615446608);

            _arc = arb.Build(out Exception ex);
            _arc.Start("XYZ: (test1) vs (test2)");
        }

    }
}
