using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Discord;
using SkyBot.Discord.CommandSystem;
using SkyBot.Discord.Data;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

namespace DiscordCommands
{
    public class EmbedCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => ResourcesCommands.EmbedCommand;

        public AccessLevel AccessLevel => AccessLevel.Admin;

        public CommandType CommandType => CommandType.Public;

        public string Description => ResourcesCommands.EmbedCommandDescription;

        public string Usage => ResourcesCommands.EmbedCommandUsage;


        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count < 3)
            {
                HelpCommand.ShowHelp(args.Channel, this);
                return;
            }

            using WebClient wc = new WebClient();

            Uri downloadUri;
            try
            {
                downloadUri = new Uri(args.Parameters[2].TrimStart('<').TrimEnd('>'));
            }
            catch (UriFormatException)
            {
                HelpCommand.ShowHelp(args.Channel, this, ResourcesCommands.EmbedCommandInvalidUri);
                return;
            }

            string json = wc.DownloadString(downloadUri);

            if (string.IsNullOrEmpty(json))
            {
                HelpCommand.ShowHelp(args.Channel, this, ResourcesCommands.EmbedCommandJsonNotFound);
                return;
            }

            EmbedJson ej = Newtonsoft.Json.JsonConvert.DeserializeObject<EmbedJson>(json);

            if (ej == null ||
                (string.IsNullOrEmpty(ej.Content) && ej.Embed == null))
            {
                HelpCommand.ShowHelp(args.Channel, this, ResourcesCommands.EmbedCommandJsonNotParsable);
                return;
            }

            DiscordEmbed embed = ej.BuildEmbed();

            if (embed == null)
            {
                HelpCommand.ShowHelp(args.Channel, this, ResourcesCommands.EmbedCommandJsonNotParsable);
                return;
            }

            try
            {
                switch (args.Parameters[0].ToLower(CultureInfo.CurrentCulture))
                {
                    case "webhook":
                        {
                            if (args.Parameters.Count < 4)
                            {
                                HelpCommand.ShowHelp(args.Channel, this);
                                return;
                            }

                            ulong chId = DiscordHandler.ExtractMentionId(args.Parameters[1], true);

                            if (chId == 0)
                            {
                                HelpCommand.ShowHelp(args.Channel, this, ResourceExceptions.CannotParseDiscordId + "channel id");
                                return;
                            }

                            string avatarUri = null;

                            if (args.Parameters.Count > 4)
                                avatarUri = args.Parameters[4].TrimStart('<').TrimEnd('>');

                            SendEmbed(this, args, embed, ej.Content, chId, args.Guild, true, args.Parameters[3], avatarUri);
                        }
                        break;

                    default:
                    case "create":
                        {
                            ulong chId = DiscordHandler.ExtractMentionId(args.Parameters[1], true);

                            if (chId == 0)
                            {
                                HelpCommand.ShowHelp(args.Channel, this, ResourceExceptions.CannotParseDiscordId + "channel id");
                                return;
                            }

                            SendEmbed(this, args, embed, ej.Content, chId, args.Guild, false, null, null);
                        }
                        break;

                    case "edit":
                        (ulong, ulong, ulong) msgUriParsed = DiscordHandler.ParseMessageLink(args.Parameters[1]);

                        if (msgUriParsed.Item1 == 0 ||
                            msgUriParsed.Item2 == 0 ||
                            msgUriParsed.Item3 == 0)
                        {
                            HelpCommand.ShowHelp(args.Channel, this, ResourceExceptions.CannotParseDiscordId + "message uri");
                            return;
                        }

                        EditEmbed(args, embed, ej.Content, msgUriParsed.Item2, msgUriParsed.Item3, args.Guild);
                        break;
                }
            }
            catch (ReadableCmdException rce)
            {
                HelpCommand.ShowHelp(args.Channel, this, rce.Message);
            }
        }


        private static void EditEmbed(CommandEventArg args, DiscordEmbed embed, string content, ulong channelId, ulong messageId, DiscordGuild guild)
        {
            DiscordChannel channel;
            DiscordMessage message;
            try
            {
                channel = guild.GetChannel(channelId);
                message = channel.GetMessageAsync(messageId).ConfigureAwait(false).GetAwaiter().GetResult();
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                throw new ReadableCmdException(ResourcesCommands.EmbedCommandChannelNotFound);
            }

            message.ModifyAsync(string.IsNullOrEmpty(content) ? default : content, embed).ConfigureAwait(false).GetAwaiter().GetResult();

            args.Channel.SendMessageAsync(ResourcesCommands.EmbedCommandModified);
        }

        private static void SendEmbed(ICommand cmd, CommandEventArg args, DiscordEmbed embed, string content, ulong channelId, DiscordGuild guild, bool webhook, string webhookUser, string webhookAvatar = null)
        {
            DiscordChannel channel;
            try
            {
                channel = guild.GetChannel(channelId);
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                throw new ReadableCmdException(ResourcesCommands.EmbedCommandChannelNotFound);
            }

            if (webhook)
            {
                try
                {
                    using (WebHookHandler whh = new WebHookHandler(channel, webhookUser, webhookAvatar))
                    {
                        whh.SendEmbed(content, new DiscordEmbed[] { embed }).ConfigureAwait(false).GetAwaiter().GetResult();
                    }
                }
                catch (UriFormatException)
                {
                    HelpCommand.ShowHelp(args.Channel, cmd, ResourcesCommands.EmbedCommandInvalidUri);
                    return;
                }
            }
            else
            {
                channel.SendMessageAsync(content: content, embed: embed).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            args.Channel.SendMessageAsync(ResourcesCommands.EmbedCommandSent).ConfigureAwait(false);
        }
    }
}
