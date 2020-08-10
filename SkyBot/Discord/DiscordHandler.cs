﻿using DSharpPlus;
using System;
using System.Collections.Generic;
using System.Text;
using SkyBot.Discord.CommandSystem;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using SkyBot.Database.Models;
using System.Linq;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;

namespace SkyBot.Discord
{
    public sealed class DiscordHandler : IDisposable
    {
        public DiscordClient Client { get; private set; }
        public CommandHandler CommandHandler { get; private set; }
        public bool IsDisposed { get; private set; }
        public bool IsReady { get; private set; }

        public DiscordHandler(string token, char commandPrefix = '!')
        {
            if (string.IsNullOrEmpty(token) || token.Length < 10)
                throw new ArgumentException(Resources.DiscordTokenEmptyNullShort, nameof(token));

            Client = new DiscordClient(new DiscordConfiguration()
            {
                TokenType = TokenType.Bot,
                Token = token,
                AutoReconnect = true
            });

            Client.Ready += e => Task.Run(() => OnClientReady());
            Client.MessageCreated += e => Task.Run(async () => await OnClientMessageCreated(e).ConfigureAwait(false));
            Client.GuildMemberAdded += e => Task.Run(async () => await OnGuildMemberCreated(e).ConfigureAwait(false));

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
            await Client.ConnectAsync().ConfigureAwait(false);
        }

        public void Start()
        {
            StartAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task OnGuildMemberCreated(GuildMemberAddEventArgs args)
        {
            using DBContext c = new DBContext();
            DiscordGuildConfig dgc = c.DiscordGuildConfig.FirstOrDefault(dgc => dgc.GuildId == (long)args.Guild.Id);

            if (dgc == null || string.IsNullOrEmpty(dgc.WelcomeMessage) || dgc.WelcomeChannel == 0)
                return;
            try
            {
                string parsedMessage = dgc.WelcomeMessage.Replace("{mention}", args.Member.Mention, StringComparison.CurrentCultureIgnoreCase);
                var dchannel = args.Guild.GetChannel((ulong)dgc.WelcomeChannel);

                await dchannel.SendMessageAsync(parsedMessage).ConfigureAwait(false);
                
            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogLevel.Error);
            }
        }

        public async Task StopAsync()
        {
            IsReady = false;

            await Client.DisconnectAsync().ConfigureAwait(false);
        }

        public void Stop()
        {
            StopAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private async Task OnClientMessageCreated(MessageCreateEventArgs e)
        {
            try
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
                        await Task.Run(() => InvokeAnalyzer(e, dgc, c)).ConfigureAwait(false);
                        return;
                    }
                }

                await Task.Run(() => CommandHandler.Invoke(e)).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogLevel.Error);
                throw;
            }
        }

        private static void InvokeAnalyzer(MessageCreateEventArgs e, DiscordGuildConfig dgc, DBContext c)
        {
            try
            {
                string[] lines = e.Message.Content.Split('\n');

                string stage = lines[0].Split('-')[1].Trim(' ');
                string mpLink = lines[1].Split(' ')[2].Trim('>').Trim('<');

                var history = OsuHistoryEndPoint.GetData.FromUrl(mpLink, null);

                var warmupMaps = c.WarmupBeatmaps.Where(wb => wb.DiscordGuildId == dgc.GuildId);

                var result = Analyzer.OsuAnalyzer.CreateStatistic(history, e.Guild, (int)(history.CurrentGameId ?? 0), dgc.AnalyzeWarmupMatches, stage, true, beatmapsToIgnore: warmupMaps.Select(wm => wm.BeatmapId).ToArray());
                var embed = Analyzer.OsuAnalyzer.GetMatchResultEmbed(result.MatchId);

                e.Channel.SendMessageAsync(embed: embed);
            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogLevel.Error);
                throw;
            }
        }

        private void OnClientReady()
        {
            IsReady = true;
            Logger.Log("Discord Client ready");
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsReady = false;

            Client?.DisconnectAsync().Wait();
            Client?.Dispose();
            Client = null;

            CommandHandler?.Dispose();
            CommandHandler = null;

            GC.Collect();
            GC.SuppressFinalize(this);

            IsDisposed = true;
        }

        public async Task<DiscordUser> GetUserAsync(ulong id)
        {
            return await RunWithNotFoundTryCatch(new Func<ulong, DiscordClient, Task<DiscordUser>>(async (u, c) =>
            {
                return await c.GetUserAsync(u).ConfigureAwait(false);
            }), id, Client).ConfigureAwait(false);
        }

        /// <summary>
        /// Runs code with a try catch designed to convert NotFoundExceptions to null
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
#pragma warning disable CA1715 // Identifiers should have correct prefix
        public static async Task<O> RunWithNotFoundTryCatch<I, C, O>(Func<I, C, Task<O>> function, I input, C from)
#pragma warning restore CA1715 // Identifiers should have correct prefix
        {
            if (function == null)
                throw new ArgumentNullException(nameof(function));
            else if (!input.GetType().IsValueType && input == null)
                throw new ArgumentNullException(nameof(input));

            try
            {
                return await function(input, from).ConfigureAwait(false);
            }
            catch (AggregateException aex)
            {
                if (!aex.InnerExceptions.Any(e => e is NotFoundException))
                    throw;
            }
            catch (NotFoundException)
            {

            }

            return default;
        }
    }
}
