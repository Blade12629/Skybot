﻿using OsuHistoryEndPoint.Data;
using SkyBot.Database;
using SkyBot.Discord;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot
{
    public static class Program
    {
        public static DiscordHandler DiscordHandler { get; private set; }
        public static Random Random { get; } = new Random();
        /// <summary>
        /// Osu irc 
        /// </summary>
        public static Osu.IRC.OsuIrcClient IRC { get; private set; }
        /// <summary>
        /// Mention of the discord bot user
        /// </summary>
        public static string BotMention => DiscordHandler.Client.CurrentUser.Mention;
        public static MaintenanceScanner MaintenanceScanner { get; private set; }


        private static void Main(string[] args)
            => MainTask(args).ConfigureAwait(false).GetAwaiter().GetResult();

        private static async Task MainTask(string[] args)
        {
            try
            {
                Logger.Log("Starting Skybot", LogLevel.Info);

                //string json;
                //History history;
                //using (System.Net.WebClient wc = new System.Net.WebClient())
                //{
                //    json = wc.DownloadString("https://osu.ppy.sh/community/matches/62010884/history?before=1410907111");
                //    history = Newtonsoft.Json.JsonConvert.DeserializeObject<History>(json);

                //    json = wc.DownloadString("https://osu.ppy.sh/community/matches/62010884/history");
                //    History history2 = Newtonsoft.Json.JsonConvert.DeserializeObject<History>(json);

                //    List<HistoryEvent> events = new List<HistoryEvent>(history.Events);
                //    events.AddRange(history2.Events);

                //    history.Events = events.ToArray();

                //    List<HistoryUser> users = new List<HistoryUser>(history.Users);
                //    users.AddRange(history2.Users);

                //    history.Users = users.ToArray();
                //}

                //json = Newtonsoft.Json.JsonConvert.SerializeObject(history, Newtonsoft.Json.Formatting.Indented);
                //System.IO.File.WriteAllText("history.json", json);

                //await Task.Delay(-1).ConfigureAwait(false);

                LoadSettings();

                if (args != null && args.Length > 0)
                {
                    using (DBContext c = new DBContext())
                    {
                        switch (args[0].ToLower(CultureInfo.CurrentCulture))
                        {
                            case "createdefaultdb":
                                c.CreateDefaultTables();
                                c.SaveChanges();
                                break;
                        }

                    }
                }

                await LoadDiscord().ConfigureAwait(false);
                //await LoadIrc().ConfigureAwait(false);

                Logger.Log("Skybot started", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString() + "\n\nPress 'x' to exit or any other key to continue");

                char pressed = Console.ReadKey().KeyChar;

                if (char.ToLower(pressed, CultureInfo.CurrentCulture).Equals('x'))
                    Environment.Exit(1);
            }

            await Task.Delay(-1).ConfigureAwait(false);
        }

        /// <summary>
        /// Loads the <see cref="SkyBotConfig"/>
        /// </summary>
        private static void LoadSettings()
        {
            Logger.Log("Loading settings", LogLevel.Info);

            if (!SkyBotConfig.Read())
            {
                SkyBotConfig.Write();

                Logger.Log("Config not found, created default one, press any key to exit", LogLevel.Error);

                Console.ReadKey();
                Environment.Exit(1);
                return;
            }
            else
                Logger.Log("Config loaded");

            Logger.Log("Loaded settings", LogLevel.Info);
        }

        /// <summary>
        /// Loads <see cref="Osu.IRC.OsuIrcClient"/>
        /// </summary>
        private static async Task LoadIrc()
        {
            Logger.Log("Loading IRC", LogLevel.Info);

            IRC = new Osu.IRC.OsuIrcClient();
            IRC.SetAuthentication(SkyBotConfig.IrcUser,
                                  SkyBotConfig.IrcPass);

            IRC.OnIrcException += (s, e) => Logger.Log("IRC Exception: " + e, LogLevel.Error, member: "IRC");
            IRC.OnUserCommand += (s, e) =>
            {
                //TODO: enable again after tests
                return;

                Logger.Log($"User command from {e.Sender.Nickname.ToString()}: {e.Message.ToString()}", member: "IRC");

                string msg = e.Message.ToString();
                int index = msg.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);

                if (index == -1)
                    return;

                VerificationManager.FinishVerification(msg.Remove(0, index + 1), e.Sender.Nickname.ToString());
            };
            IRC.OnWelcomeMessageReceived += (s, e) => Logger.Log($"Welcome message received", member: "IRC");

            await IRC.ConnectAndLoginAsync().ConfigureAwait(false);

            Logger.Log("Loaded IRC", LogLevel.Info);
        }

        /// <summary>
        /// Loads <see cref="Discord.DiscordHandler"/> and <see cref="Discord.CommandSystem.CommandHandler"/>
        /// </summary>
        private static async Task LoadDiscord()
        {
            Logger.Log("Loading Discord", LogLevel.Info);

            DiscordHandler = new DiscordHandler(SkyBotConfig.DiscordToken);
            DiscordHandler.Client.Ready += s => Task.Run(() => LoadMaintenanceScanner());
            await DiscordHandler.StartAsync().ConfigureAwait(false);

            Logger.Log("Loaded Discord", LogLevel.Info);
        }

        /// <summary>
        /// Loads the <see cref="MaintenanceScanner"/>
        /// </summary>
        private static void LoadMaintenanceScanner()
        {
            MaintenanceScanner = new MaintenanceScanner(TimeSpan.FromSeconds(10));
            MaintenanceScanner.OnMaintenanceChanged += OnMaintenanceChanged;

            MaintenanceScanner.Start();
        }

        /// <summary>
        /// Checks if our maintenance status has changed
        /// </summary>
        /// <param name="e">Status, StatusMessage</param>
        private static void OnMaintenanceChanged(object sender, (bool, string) e)
        {
            DiscordHandler.Client.UpdateStatusAsync(new DSharpPlus.Entities.DiscordGame(e.Item2), e.Item1 ? DSharpPlus.Entities.UserStatus.DoNotDisturb : DSharpPlus.Entities.UserStatus.Online);
        }
    }
}
