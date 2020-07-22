using SkyBot.Database;
using SkyBot.Discord;
using System;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot
{
    public class Program
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

                await LoadSettings();
                await LoadDiscord();
                await LoadIrc();

                Logger.Log("Skybot started", LogLevel.Info);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.ToString() + "\n\nPress 'x' to exit or any other key to continue");

                char pressed = Console.ReadKey().KeyChar;

                if (char.ToLower(pressed).Equals('x'))
                    Environment.Exit(1);
            }

            await Task.Delay(-1);
        }

        /// <summary>
        /// Loads the <see cref="SkyBotConfig"/>
        /// </summary>
        private static async Task LoadSettings()
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
                Logger.Log($"User command from {e.Sender.Nickname.ToString()}: {e.Message.ToString()}", member: "IRC");
                VerificationManager.FinishVerification(e.Message.ToString(), e.Sender.Nickname.ToString());
            };
            IRC.OnWelcomeMessageReceived += (s, e) => Logger.Log($"Welcome message received", member: "IRC");

            await IRC.ConnectAndLoginAsync();

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
            await DiscordHandler.StartAsync();

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

        /// <summary>
        /// Generates a MD5 hash string
        /// </summary>
        /// <param name="input">String to compute md5 from</param>
        /// <returns>MD5 hash</returns>
        public static string GenerateMd5(string input)
        {
            using (System.Security.Cryptography.MD5 md = System.Security.Cryptography.MD5.Create())
            {
                byte[] data = md.ComputeHash(Encoding.UTF8.GetBytes(input));

                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < data.Length; i++)
                    builder.Append(data[i].ToString("x2"));

                return builder.ToString();
            }
        }

    }
}
