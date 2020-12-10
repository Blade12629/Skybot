using AutoRefTypes;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Osu.AutoRef.Tools
{
    public class ARDiscordHandler : IDiscordHandler
    {
        DiscordHandler _discord;
        ulong _guildId;

        public ARDiscordHandler(DiscordHandler discord, ulong guildId)
        {
            _discord = discord;
            _guildId = guildId;
        }

        public void SendEmbed(ulong channel, string title, string description)
        {
            var dchannel = _discord.GetChannelAsync(_guildId, channel).ConfigureAwait(false).GetAwaiter().GetResult();

            if (dchannel == null)
                return;

            _discord.SendSimpleEmbed(dchannel, title, description).ConfigureAwait(false);
        }

        public void SendEmbed(ulong channel, string title, string description, params (string, string, bool)[] fields)
        {
            var dchannel = _discord.GetChannelAsync(_guildId, channel).ConfigureAwait(false).GetAwaiter().GetResult();

            if (dchannel == null)
                return;

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = title,
                Description = description
            };

            if (fields != null && fields.Length > 0)
                for (int i = 0; i < fields.Length; i++)
                    builder.AddField(fields[i].Item1, fields[i].Item2, fields[i].Item3);

            dchannel.SendMessageAsync(embed: builder.Build()).ConfigureAwait(false);
        }

        public void SendMessage(ulong channel, string message)
        {
            var dchannel = _discord.GetChannelAsync(_guildId, channel).ConfigureAwait(false).GetAwaiter().GetResult();

            if (dchannel == null)
                return;

            dchannel.SendMessageAsync(message).ConfigureAwait(false);
        }
    }
}
