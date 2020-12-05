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
            //args.ParameterString = args.ParameterString.Remove(0, args.Parameters[0].Length + 1);
            //args.Parameters.RemoveAt(0);

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

                case "-create":
                    {
                        string script = System.IO.File.ReadAllText("Osu\\AutoRef\\ExampleScript\\TestScript.js");

                        AutoRefBuilder arb = new AutoRefBuilder(Program.IRC, script)
                        {
                            CaptainBlue = "Skyfly"
                        };


                        _arc = arb.Build(out Exception ex);

                        if (ex != null)
                        {
                            Logger.Log(ex);
                            return;
                        }

                        args.Channel.SendMessageAsync("Created match").ConfigureAwait(false);
                    }
                    return;

                case "-stop":
                    {
                        _arc.LC.EnqueueCloseLobby();
                    }
                    args.Channel.SendMessageAsync("Stopped match").ConfigureAwait(false);
                    return;

                case "-close":
                    Program.IRC.JoinChannelAsync($"#mp_{args.Parameters[1]}").ConfigureAwait(false).GetAwaiter().GetResult();
                    Program.IRC.SendMessageAsync($"#mp_{args.Parameters[1]}", "!mp close").ConfigureAwait(false);
                    return;

                case "-start":
                    _arc.Start("XYZ: (test1) vs (test2)");
                    args.Channel.SendMessageAsync("Started match").ConfigureAwait(false);
                    return;
            }
        }
    }
}
