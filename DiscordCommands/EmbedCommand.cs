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

        public string Command => "embed";

        public AccessLevel AccessLevel => AccessLevel.Admin;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Create or edit embeds";

        public string Usage =>  "{prefix}embed create <channel> <urlToEmbedJson>\n" +
                                "{prefix}embed edit <messageLink> <urlToEmbedJson>\n" +
                                "{prefix}embed webhook <channel> <urlToEmbedJson> <username> [avatarLink]\n\n" +
                                "Embed Visualizer: <https://leovoel.github.io/embed-visualizer/>";

        public int MinParameters => 3;
        public bool AllowOverwritingAccessLevel => true;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            using WebClient wc = new WebClient();

            Uri downloadUri;
            try
            {
                downloadUri = new Uri(args.Parameters[2].TrimStart('<').TrimEnd('>'));
            }
            catch (UriFormatException)
            {
                HelpCommand.ShowHelp(args.Channel, this, "Invalid link");
                return;
            }

            string json = wc.DownloadString(downloadUri);

            if (string.IsNullOrEmpty(json))
            {
                HelpCommand.ShowHelp(args.Channel, this, "Embed json not found");
                return;
            }

            EmbedJson ej = Newtonsoft.Json.JsonConvert.DeserializeObject<EmbedJson>(json);

            if (ej == null ||
                (string.IsNullOrEmpty(ej.Content) && ej.Embed == null))
            {
                HelpCommand.ShowHelp(args.Channel, this, "Failed to parse embed json");
                return;
            }

            DiscordEmbed embed = ej.BuildEmbed();

            if (embed == null)
            {
                HelpCommand.ShowHelp(args.Channel, this, "Failed to parse embed json");
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

                            SendEmbed(client, this, args, embed, ej.Content, chId, args.Guild, true, args.Parameters[3], avatarUri);
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

                            SendEmbed(client, this, args, embed, ej.Content, chId, args.Guild, false, null, null);
                        }
                        break;

                    case "edit":
                        DiscordMessageLink msgLink = DiscordHandler.ExtractMessageLink(args.Parameters[1]);

                        if (msgLink.DiscordGuildId == 0 ||
                            msgLink.DiscordChannelId == 0 ||
                            msgLink.DiscordMessageId == 0)
                        {
                            HelpCommand.ShowHelp(args.Channel, this, ResourceExceptions.CannotParseDiscordId + "message uri");
                            return;
                        }

                        EditEmbed(client, args, embed, ej.Content, msgLink.DiscordChannelId, msgLink.DiscordMessageId, args.Guild);
                        break;
                }
            }
            catch (ReadableCmdException rce)
            {
                HelpCommand.ShowHelp(args.Channel, this, rce.Message);
            }
        }


        private static void EditEmbed(DiscordHandler client, CommandEventArg args, DiscordEmbed embed, string content, ulong channelId, ulong messageId, DiscordGuild guild)
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
                throw new ReadableCmdException("Could not find the discord channel");
            }

            message.ModifyAsync(string.IsNullOrEmpty(content) ? default : content, embed).ConfigureAwait(false).GetAwaiter().GetResult();

            client.SendSimpleEmbed(args.Channel, "Embed modified").ConfigureAwait(false);
        }

        /// <summary>
        /// Sends an embed to a specific channel
        /// </summary>
        /// <param name="content">message content</param>
        /// <param name="webhook">Use webhook or message</param>
        /// <param name="webhookUser">Webhook username</param>
        private static void SendEmbed(DiscordHandler client, ICommand cmd, CommandEventArg args, DiscordEmbed embed, string content, ulong channelId, DiscordGuild guild, bool webhook, string webhookUser, string webhookAvatar = null)
        {
            DiscordChannel channel;
            try
            {
                channel = guild.GetChannel(channelId);
            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {
                throw new ReadableCmdException("Could not find the discord channel");
            }

            //TODO: make it so webhook and default can both send up to 5 embeds max at once

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
                    HelpCommand.ShowHelp(args.Channel, cmd, "Invalid link");
                    return;
                }
            }
            else
            {
                channel.SendMessageAsync(content: content, embed: embed).ConfigureAwait(false).GetAwaiter().GetResult();
            }

            client.SendSimpleEmbed(args.Channel, "Sent Embed").ConfigureAwait(false);
        }
    }
}
