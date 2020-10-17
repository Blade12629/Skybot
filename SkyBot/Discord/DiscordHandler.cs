using DSharpPlus;
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
using System.Globalization;
using SkyBot;
using LogLevel = SkyBot.LogLevel;
using SkyBot.Discord;

public sealed class DiscordHandler : IDisposable
{
    public DiscordClient Client { get; private set; }
    public CommandHandler CommandHandler { get; private set; }
    public bool IsDisposed { get; private set; }
    public bool IsReady { get; private set; }
    public IReadOnlyDictionary<ulong, DiscordGuild> Guilds => Client.Guilds;

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
            User user = c.User.FirstOrDefault(u => u.DiscordUserId == (long)args.Member.Id);
            long osuId = user == null ? 0 : user.OsuUserId;

            List<BannedUser> bans = BanManager.GetBansForUser((long)args.Member.Id, osuId, args.Guild == null ? 0 : (long)args.Guild.Id);

            if (bans.Count > 0)
            {
                if (dgc.DebugChannel != 0)
                    await args.Guild.GetChannel((ulong)dgc.DebugChannel).SendMessageAsync($"Banned user detected ({args.Member.Mention} ({args.Member.Id})").ConfigureAwait(false);

                if (dgc.BlacklistRoleId != 0)
                {
                    var drole = args.Guild.GetRole((ulong)dgc.BlacklistRoleId);
                    await args.Member.GrantRoleAsync(drole, "blacklisted").ConfigureAwait(false);
                }

                return;
            }

            string parsedMessage = dgc.WelcomeMessage.Replace("{mention}", args.Member.Mention, StringComparison.CurrentCultureIgnoreCase);
            var dchannel = args.Guild.GetChannel((ulong)dgc.WelcomeChannel);

            await dchannel.SendMessageAsync(parsedMessage).ConfigureAwait(false);
            await VerificationManager.SynchronizeVerification(args.Member.Id, args.Guild.Id, dgc).ConfigureAwait(false);
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
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    Task.Run(() => InvokeAnalyzer(e, dgc)).ConfigureAwait(false);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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

    private static void InvokeAnalyzer(MessageCreateEventArgs e, DiscordGuildConfig dgc)
    {
        try
        {
            using DBContext c = new DBContext();

            string[] lines = e.Message.Content.Split('\n');

            string stageLine = lines.FirstOrDefault(l => l.StartsWith("Stage -", StringComparison.CurrentCultureIgnoreCase));

            if (!string.IsNullOrEmpty(stageLine))
            {
                int stageIndex = stageLine.IndexOf("-", StringComparison.CurrentCultureIgnoreCase);

                string stage = stageLine.Remove(0, stageIndex + 1).TrimStart(' ').Trim(' ');
                string mpLink = lines.FirstOrDefault(l => l.StartsWith("MP link:", StringComparison.CurrentCultureIgnoreCase)).Split(' ')[2].Trim('>').Trim('<');

                if (!string.IsNullOrEmpty(mpLink))
                {
                    var history = OsuHistoryEndPoint.GetData.FromUrl(mpLink, null);

                    var warmupMaps = c.WarmupBeatmaps.Where(wb => wb.DiscordGuildId == dgc.GuildId);

                    var result = SkyBot.Analyzer.OsuAnalyzer.CreateStatistic(history, e.Guild, (int)(history.CurrentGameId ?? 0), dgc.AnalyzeWarmupMatches, stage, true, beatmapsToIgnore: warmupMaps.Select(wm => wm.BeatmapId).ToArray());
                    var embed = SkyBot.Analyzer.OsuAnalyzer.GetMatchResultEmbed(result.MatchId);

                    e.Channel.SendMessageAsync(embed: embed);
                }
            }
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

        GC.SuppressFinalize(this);

        IsDisposed = true;
    }

    /// <summary>
    /// Runs code with a try catch designed to convert NotFoundExceptions to null
    /// </summary>
    /// <exception cref="ArgumentNullException"></exception>
#pragma warning disable CA1715 // Identifiers should have correct prefix
#pragma warning disable CA1822 // Mark members as static
    private void RunActionWithTryCatch(Action ac)
    {
        try
        {
            ac();
        }
        catch (AggregateException aex)
        {
            if (!aex.InnerExceptions.Any(e => e is NotFoundException ||
                                              e is UnauthorizedException))
                throw;
        }
        catch (NotFoundException nfe)
        {

        }
        catch (UnauthorizedException ue)
        {

        }
    }

    private T RunFuncWithTryCatch<T>(Func<T> func)
    {
        try
        {
            return func();
        }
        catch (AggregateException aex)
        {
            if (!aex.InnerExceptions.Any(e => e is NotFoundException ||
                                              e is UnauthorizedException))
                throw;
        }
        catch (NotFoundException nfe)
        {

        }
        catch (UnauthorizedException ue)
        {

        }

        return default;
    }

    private T RunFuncWithTryCatch<I, T>(Func<I, T> func, I input)
    {
        try
        {
            return func(input);
        }
        catch (AggregateException aex)
        {
            if (!aex.InnerExceptions.Any(e => e is NotFoundException ||
                                              e is UnauthorizedException))
                throw;
        }
        catch (NotFoundException nfe)
        {

        }
        catch (UnauthorizedException ue)
        {

        }

        return default;
    }
#pragma warning restore CA1715 // Identifiers should have correct prefix
#pragma warning restore CA1822 // Mark members as static

#pragma warning disable CA1822 // Mark members as static
    /// <summary>
    /// Parses a discord message link into their ids
    /// </summary>
    /// <param name="message"></param>
    /// <returns>GuildId (0 if private chat), ChannelId, MessageId</returns>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="FormatException"></exception>
    public static DiscordMessageLink ExtractMessageLink(string message)
    {
        const string CH_START = "channels/";

        if (string.IsNullOrEmpty(message))
            throw new ArgumentNullException(nameof(message));

        int startIndex = message.IndexOf(CH_START, StringComparison.CurrentCultureIgnoreCase);

        if (startIndex == -1)
            throw new FormatException(ResourceExceptions.CannotParseMessageLink);

        message = message.Remove(0, startIndex + 1 + CH_START.Length);

        string[] split = message.Split('/');

        ulong[] parsed = new ulong[3];

        for (int i = 0; i < 3; i++)
            if (ulong.TryParse(split[i], out ulong id))
                parsed[i] = id;

        return new DiscordMessageLink(parsed[0], parsed[1], parsed[2]);
    }

    /// <summary>
    /// Extracts the ID from a <paramref name="mention"/> <see cref="string"/>
    /// </summary>
    /// <param name="channel">Is channel or user</param>
    /// <returns>Id or 0 if unable to parse</returns>
    public static ulong ExtractMentionId(string mention, bool channel = false)
    {
        if (string.IsNullOrEmpty(mention))
            throw new ArgumentNullException(nameof(mention));
        else if ((channel && mention.Length < 4) ||
                    (!channel && mention.Length < 5))
            return 0;

        if (channel)
            mention = mention.Trim('<', '#', '>', ' ');
        else
            mention = mention.Trim('<', '@', '!', '>', ' ');

        if (ulong.TryParse(mention, out ulong id))
            return id;

        return 0;
    }

    /// <summary>
    /// Parses the <paramref name="id"/> into a mention <see cref="string"/>
    /// </summary>
    /// <param name="channel">Is channel or user</param>
    /// <returns></returns>
    public static string ParseMentionId(ulong id, bool channel = false)
    {
        if (channel)
            return $"<#{id}>";
        else
            return $"<@!{id}>";
    }


    /// <summary>
    /// Gets an embed which shows the current amount of <see cref="DiscordGuild"/>s, Verified <see cref="User"/>s and the current bot uptime
    /// </summary>
    public DiscordEmbed GetBotInfo()
    {
        DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
        {
            Title = $"{Resources.BotInfoFor} Skybot",
            Description = "‎"
        };

        using DBContext c = new DBContext();

        builder.AddField(Resources.DiscordGuilds, Client.Guilds.Count.ToString(CultureInfo.CurrentCulture), true)
                .AddField(Resources.VerifiedUsers, c.User.Count().ToString(CultureInfo.CurrentCulture), true)
                .AddField(Resources.LastUpdateDate, Program.LastUpdatedOn.ToString(CultureInfo.CurrentCulture))
                .AddField(Resources.Uptime, DateTime.UtcNow.Subtract(Program.StartedOn).ToString());

        return builder.Build();
    }

    /// <summary>
    /// Sends a simple embed that only contains a title and optional a description
    /// </summary>
    public async Task<DiscordMessage> SendSimpleEmbed(DiscordChannel channel, string title, string description = null)
    {
        if (channel == null)
            throw new ArgumentNullException(nameof(channel));
        else if (string.IsNullOrEmpty(title))
            throw new ArgumentNullException(nameof(title));

        DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
        {
            Title = title,
            Description = description
        };

        return await channel.SendMessageAsync(embed: builder.Build()).ConfigureAwait(false);
    }

    public async Task<DiscordGuild> GetGuildAsync(ulong id)
    {
        return await RunFuncWithTryCatch(async () =>
        {
            return Client.GetGuildAsync(id).ConfigureAwait(false).GetAwaiter().GetResult();
        }).ConfigureAwait(false);
    }

    public async Task<DiscordChannel> GetChannelAsync(DiscordGuild guild, ulong id)
    {
        return await RunFuncWithTryCatch(async () =>
        {
            return await Task.Run(() => guild.GetChannel(id)).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task<DiscordChannel> GetChannelAsync(ulong guildId, ulong id)
    {
        DiscordGuild guild = await GetGuildAsync(guildId).ConfigureAwait(false);

        if (guild == null)
            return null;

        return await GetChannelAsync(guild, id).ConfigureAwait(false);
    }

    public async Task<DiscordMessage> GetMessageAsync(DiscordChannel channel, ulong id)
    {
        return await RunFuncWithTryCatch(async () =>
        {
            return await channel.GetMessageAsync(id).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task<DiscordMessage> GetMessageAsync(DiscordGuild guild, ulong channelId, ulong id)
    {
        DiscordChannel channel = await GetChannelAsync(guild, channelId).ConfigureAwait(false);

        if (channel == null)
            return null;

        return await GetMessageAsync(channel, id).ConfigureAwait(false);
    }

    public async Task<DiscordMessage> GetMessageAsync(ulong guildId, ulong channelId, ulong id)
    {
        DiscordGuild guild = await GetGuildAsync(guildId).ConfigureAwait(false);

        if (guild == null)
            return null;

        return await GetMessageAsync(guild, channelId, id).ConfigureAwait(false);
    }

    public async Task<DiscordMember> GetMemberAsync(DiscordGuild guild, ulong id)
    {
        return await RunFuncWithTryCatch(async () =>
        {
            return await guild.GetMemberAsync(id).ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task<DiscordMember> GetMemberAsync(ulong guildId, ulong id)
    {
        DiscordGuild guild = await GetGuildAsync(guildId).ConfigureAwait(false);

        if (guild == null)
            return null;

        return await GetMemberAsync(guild, id).ConfigureAwait(false);
    }

    public async Task<DiscordDmChannel> GetDmChannelAsync(DiscordMember member)
    {
        return await RunFuncWithTryCatch(async () =>
        {
            return await member.CreateDmChannelAsync().ConfigureAwait(false);
        }).ConfigureAwait(false);
    }

    public async Task<DiscordDmChannel> GetDmChannelAsync(DiscordGuild guild, ulong id)
    {
        DiscordMember member = await GetMemberAsync(guild, id).ConfigureAwait(false);

        if (member == null)
            return null;

        return await GetDmChannelAsync(member).ConfigureAwait(false);
    }

    public async Task<DiscordDmChannel> GetDmChannelAsync(ulong guildId, ulong id)
    {
        DiscordGuild guild = await GetGuildAsync(guildId).ConfigureAwait(false);

        if (guild == null)
            return null;

        return await GetDmChannelAsync(guild, id).ConfigureAwait(false);
    }

    public async Task<DiscordWebhook> CreateWebhookAsync(DiscordChannel channel, string name, System.IO.Stream stream = null, string reason = null)
    {
        return await RunFuncWithTryCatch(async () => await channel.CreateWebhookAsync(name, stream, reason).ConfigureAwait(false)).ConfigureAwait(false);
    }

    public async Task<DiscordWebhook> CreateWebhookAsync(DiscordGuild guild, ulong channelId, string name, System.IO.Stream stream = null, string reason = null)
    {
        DiscordChannel channel = await GetChannelAsync(guild, channelId).ConfigureAwait(false);

        if (channel == null)
            return null;

        return await CreateWebhookAsync(channel, name, stream, reason).ConfigureAwait(false);
    }

    public async Task<DiscordWebhook> CreateWebhookAsync(ulong guildId, ulong channelId, string name, System.IO.Stream stream = null, string reason = null)
    {
        DiscordGuild guild = await GetGuildAsync(guildId).ConfigureAwait(false);

        if (guild == null)
            return null;

        return await CreateWebhookAsync(guild, channelId, name, stream, reason).ConfigureAwait(false);
    }

    public async Task DeleteWebhookAsync(DiscordWebhook webhook)
    {
        await Task.Run(() => RunActionWithTryCatch(async () => await webhook.DeleteAsync().ConfigureAwait(false))).ConfigureAwait(false);
    }

    public async Task<DiscordUser> GetUserAsync(ulong id)
    {
        return await RunFuncWithTryCatch(async () => await Client.GetUserAsync(id).ConfigureAwait(false)).ConfigureAwait(false);
    }

    public async Task<DiscordChannel> GetChannelAsync(ulong id)
    {
        return await RunFuncWithTryCatch(async () => await Client.GetChannelAsync(id).ConfigureAwait(false)).ConfigureAwait(false);
    }

    public async Task<DiscordDmChannel> GetDmChannelAsync(ulong userId)
    {
        DiscordUser user = await GetUserAsync(userId).ConfigureAwait(false);

        if (user == null)
            return null;

        return await GetDmChannelAsync(user).ConfigureAwait(false);
    }

    public async Task<DiscordDmChannel> GetDmChannelAsync(DiscordUser user)
    {
        return await RunFuncWithTryCatch(async () => await Client.CreateDmAsync(user).ConfigureAwait(false)).ConfigureAwait(false);
    }

#pragma warning restore CA1822 // Mark members as static
}
