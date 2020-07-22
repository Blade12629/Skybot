using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;
using SkyBot.Discord.CommandSystem;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using SkyBot.Database.Models;
using System.Linq;

namespace SkyBot.Discord
{
    public class DiscordHandler : IDisposable
    {
        public DiscordClient Client { get; private set; }
        public CommandHandler CommandHandler { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool IsReady { get; private set; }

        public DiscordHandler(string token, char commandPrefix = '!')
        {
            if (string.IsNullOrEmpty(token) || token.Length < 10)
                throw new ArgumentException("Token cannot be null, empty or less than 10 long", nameof(token));

            Client = new DiscordClient(new DiscordConfiguration()
            {
                TokenType = TokenType.Bot,
                Token = token,
                AutoReconnect = true
            });

            Client.Ready += async e => await OnClientReady(e);
            Client.MessageCreated += async e => await OnClientMessageCreated(e);

            CommandHandler = new CommandHandler(this, commandPrefix);
        }

        ~DiscordHandler()
        {
            Dispose();
        }

        public async Task StartAsync()
        {
            Logger.Log("Starting discord client");

            CommandHandler.LoadCommands("DiscordCommands.dll");
            await Client.ConnectAsync();
        }

        public void Start()
        {
            StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task StopAsync()
        {
            IsReady = false;

            await Client.DisconnectAsync();
        }

        public void Stop()
        {
            StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task OnClientMessageCreated(MessageCreateEventArgs e)
        {
            //Ignore bot messages
            if (e.Author.Id == Client.CurrentUser.Id || string.IsNullOrEmpty(e.Message?.Content ?? ""))
                return;

            if (e.Guild != null)
            {
                using DBContext c = new DBContext();
                DiscordGuildConfig dgc = c.DiscordGuildConfig.FirstOrDefault(dgc => dgc.GuildId == (long)e.Guild.Id);

                if (dgc != null && dgc.AnalyzeChannelId != 0 && dgc.AnalyzeChannelId == (long)e.Channel.Id)
                {
                    await Task.Run(() => InvokeAnalyzer(e, dgc, c));
                    return;
                }
            }

            await Task.Run(() => CommandHandler.Invoke(e));
        }

        private void InvokeAnalyzer(MessageCreateEventArgs e, DiscordGuildConfig dgc, DBContext c)
        {
            string[] lines = e.Message.Content.Split(Environment.NewLine);

            string stage = lines[0].Split('-')[1].Trim(' ');
            string mpLink = lines[1].Split('<')[1].Trim('>');

            var history = Analyzer.Analyzer.GetHistory(mpLink);

            var warmupMaps = c.WarmupBeatmaps.Where(wb => wb.DiscordGuildId == dgc.GuildId);

            var result = Analyzer.Analyzer.CreateStatistic(history, e.Guild, 0, warmupCount: dgc.AnalyzeWarmupMatches, stage, true, beatmapsToIgnore: warmupMaps.Select(wm => wm.BeatmapId).ToArray());
            var embed = Analyzer.Analyzer.CreateStatisticEmbed(result, new DSharpPlus.Entities.DiscordColor(1, 0, 1));

            e.Channel.SendMessageAsync(embed: embed);
        }

        private async Task OnClientReady(ReadyEventArgs e)
        {
            IsReady = true;
            Logger.Log("Discord Client ready");
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsReady = false;

            try
            {
                Client?.DisconnectAsync().Wait();
                Client?.Dispose();
            }
            catch (Exception)
            {

            }

            Client = null;

            try
            {
                CommandHandler?.Dispose();
            }
            catch (Exception)
            {
                
            }

            CommandHandler = null;

            GC.Collect();
            GC.SuppressFinalize(this);

            IsDisposed = true;
        }
    }
}
