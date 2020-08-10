using DSharpPlus.Entities;
using SkyBot;
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

        public string Description => "Create, edit or reverse embeds";

        public string Usage =>  "```" + Environment.NewLine +
                                "!embed create channelId embedCode/-url:www.example.com/raw/text" + Environment.NewLine +
                                "!embed edit channelId messageId embedCode/-url:www.example.com/raw/text" + Environment.NewLine +
                                "!embed reverse channelId messageId" + Environment.NewLine +
                                "```";


        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            try
            {
                if (args.Guild == null)
                {
                    args.Channel.SendMessageAsync("This command is only usable in a discord channel");
                    return;
                }
                else if (args.Parameters.Count == 0)
                {
                    HelpCommand.ShowHelp(args.Channel, this);
                    return;
                }

                string afterCMD = args.ParameterString;

                string download = null;

                int urlStart = afterCMD.IndexOf("-url:", StringComparison.CurrentCultureIgnoreCase);
                string urlString = null;
                if (urlStart > 0)
                {
                    urlString = afterCMD.Remove(0, urlStart + 5);
                    if (!string.IsNullOrEmpty(urlString))
                    {
                        using WebClient wc = new WebClient();
                        download = wc.DownloadString(urlString);
                    }
                }


                string @params = afterCMD;

                if (urlString != null && download != null)
                    @params = @params.Replace("-url:" + urlString, download, StringComparison.CurrentCultureIgnoreCase);

                int index = @params.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);
                if (index <= 1)
                {
                    HelpCommand.ShowHelp(args.Channel, this);
                    return;
                }

                string cmdType = @params.Substring(0, index).TrimStart(' ').TrimEnd(' ').ToLower(CultureInfo.CurrentCulture);
                @params = @params.Remove(0, index + 1);

                string channelIdStr;
                ulong channelId;
                ulong messageId;
                DiscordChannel dchannel;
                DiscordMessage dmessage;
                EmbedJson embedJson;
                DiscordEmbed embed;
                if (cmdType.Equals("edit", StringComparison.CurrentCultureIgnoreCase))
                {
                    index = @params.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);
                    if (index <= 1)
                    {
                        HelpCommand.ShowHelp(args.Channel, this);
                        return;
                    }

                    channelIdStr = @params.Substring(0, index);

                    @params = @params.Remove(0, index + 1);

                    if (!ulong.TryParse(channelIdStr, out channelId))
                    {
                        args.Channel.SendMessageAsync("Could not parse channel id: " + channelIdStr);
                        return;
                    }

                    index = @params.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);
                    if (index <= 1)
                    {
                        HelpCommand.ShowHelp(args.Channel, this);
                        return;
                    }

                    string messageIdStr = @params.Substring(0, index);
                    @params = @params.Remove(0, index + 1);

                    if (!ulong.TryParse(messageIdStr, out messageId))
                    {
                        args.Channel.SendMessageAsync("Could not parse message id: " + messageIdStr);
                        return;
                    }

                    dchannel = Program.DiscordHandler.Client.GetChannelAsync(channelId).ConfigureAwait(false).GetAwaiter().GetResult();
                    dmessage = dchannel.GetMessageAsync(messageId).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (dmessage == null)
                    {
                        args.Channel.SendMessageAsync("Could not find message " + messageId);
                        return;
                    }

                    embedJson = Newtonsoft.Json.JsonConvert.DeserializeObject<EmbedJson>(@params);

                    if (embedJson == null)
                    {
                        args.Channel.SendMessageAsync("Failed to parse your embed json");
                        return;
                    }

                    embed = embedJson.BuildEmbed();
                    dmessage.ModifyAsync(embedJson.Content ?? default(Optional<string>), embed);

                    return;
                }
                else if (cmdType.Equals("reverse", StringComparison.CurrentCultureIgnoreCase))
                {
                    index = @params.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);
                    if (index <= 1)
                    {
                        HelpCommand.ShowHelp(args.Channel, this);
                        return;
                    }

                    channelIdStr = @params.Substring(0, index);
                    @params = @params.Remove(0, index + 1);

                    if (!ulong.TryParse(channelIdStr, out channelId))
                    {
                        args.Channel.SendMessageAsync("Could not parse channel id: " + channelIdStr);
                        return;
                    }

                    string messageIdStr = @params;

                    if (!ulong.TryParse(messageIdStr, out messageId))
                    {
                        args.Channel.SendMessageAsync("Could not parse message id: " + messageIdStr);
                        return;
                    }
                    dchannel = Program.DiscordHandler.Client.GetChannelAsync(channelId).ConfigureAwait(false).GetAwaiter().GetResult();
                    dmessage = dchannel.GetMessageAsync(messageId).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (dmessage == null)
                    {
                        args.Channel.SendMessageAsync("Could not find message " + messageId);
                        return;
                    }

                    var reversed = EmbedJson.ReverseEmbed(dmessage);
                    string json = Newtonsoft.Json.JsonConvert.SerializeObject(reversed, Newtonsoft.Json.Formatting.Indented);

                    args.Channel.SendMessageAsync("```js" + Environment.NewLine + json + Environment.NewLine + "```");
                    return;
                }

                index = @params.IndexOf(' ', StringComparison.CurrentCultureIgnoreCase);
                if (index <= 1)
                {
                    HelpCommand.ShowHelp(args.Channel, this);
                    return;
                }

                channelIdStr = @params.Substring(0, index);
                @params = @params.Remove(0, index + 1);

                if (@params.Length <= 1)
                {
                    HelpCommand.ShowHelp(args.Channel, this);
                    return;
                }

                if (!ulong.TryParse(channelIdStr, out channelId))
                {
                    args.Channel.SendMessageAsync("Could not parse channel id: " + channelIdStr);
                    return;
                }

                Logger.Log(channelIdStr + " : " + channelId);

                dchannel = Program.DiscordHandler.Client.GetChannelAsync(channelId).ConfigureAwait(false).GetAwaiter().GetResult();
                embedJson = Newtonsoft.Json.JsonConvert.DeserializeObject<EmbedJson>(@params);

                if (embedJson == null)
                {
                    args.Channel.SendMessageAsync("Failed to parse your embed json");
                    return;
                }

                embed = embedJson.BuildEmbed();
                dchannel.SendMessageAsync(embedJson.Content, false, embed);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Logger.Log(ex.ToString(), LogLevel.Error);
            }
        }

        private ulong GetChannelId(string channel)
        {
            int indexStart = channel.IndexOf('<', StringComparison.CurrentCulture);

            if (indexStart == -1)
                return 0;

            string id = channel.Remove(0, indexStart + 1);

            if (!id[0].Equals('#'))
                return 0;

            id = id.Remove(0, 1);
            indexStart = channel.IndexOf('>', StringComparison.CurrentCulture);
            if (indexStart == -1)
                return 0;
            try
            {
                id = id.Substring(0, indexStart - 1);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                id = id.Substring(0, indexStart - 2);
            }

            if (ulong.TryParse(id, out ulong result))
                return result;

            return 0;
        }
    }
}
