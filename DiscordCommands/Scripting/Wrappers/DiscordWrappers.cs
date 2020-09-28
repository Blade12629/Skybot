using DSharpPlus;
using DSharpPlus.Entities;
using SkyBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DiscordCommands.Scripting.Wrappers
{
    /// <summary>
    /// Abstract discord wrapper, cannot be used for scripting
    /// </summary>
    public abstract class DiscordWrapper<T> : JSObjectWrapper<T>
    {
        protected DiscordHandler _client;

        /// <summary>
        /// Abstract discord wrapper, cannot be used for scripting
        /// </summary>
        public DiscordWrapper(T obj, DiscordHandler client) : base(obj)
        {
            _client = client;
        }
    }

    /// <summary>
    /// Discord Guild
    /// </summary>
    public sealed class DiscordChannelWrapper : DiscordWrapper<DiscordChannel>
    {
        /// <summary>
        /// Discord Channel ID
        /// </summary>
        public string ID => _value?.Id.ToString(CultureInfo.CurrentCulture) ?? "0";

        /// <summary>
        /// Creates a <see cref="DiscordChannel"/>, for scripting use <see cref="DiscordGuildWrapper.GetChannel(string)"/>
        /// </summary>
        public DiscordChannelWrapper(DiscordChannel channel, DiscordHandler client) : base(channel, client)
        {

        }

        /// <summary>
        /// Sends a message to the channel
        /// </summary>
        /// <param name="message">Message</param>
        public void SendMessage(string message)
        {
            if (string.IsNullOrEmpty(message))
                throw new ArgumentNullException(nameof(message));

            _value.SendMessageAsync(message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends a simple embed with a title and optionally a description
        /// </summary>
        /// <param name="title">Embed Title</param>
        /// <param name="description">Embed Description</param>
        public void SendSimpleEmbed(string title, string description = null)
        {
            _client.SendSimpleEmbed(_value, title, description).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Renames the channel
        /// </summary>
        /// <param name="newName">New channel name</param>
        public void RenameChannel(string newName)
        {
            if (string.IsNullOrEmpty(newName))
                throw new ArgumentNullException(nameof(newName));

            _value.ModifyAsync(name: newName).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Sends an embed
        /// </summary>
        /// <param name="embed">Embed to send</param>
        /// <param name="message">Optionally a message above the embed</param>
        public void SendEmbed(DiscordEmbed embed, string message = null)
        {
            _value.SendMessageAsync(content: message, embed: embed).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }

    /// <summary>
    /// Discord Member
    /// </summary>
    public sealed class DiscordGuildWrapper : DiscordWrapper<DiscordGuild>
    {
        /// <summary>
        /// Discord Guild ID
        /// </summary>
        public string ID => _value?.Id.ToString(CultureInfo.CurrentCulture) ?? "0";

        /// <summary>
        /// Creates a <see cref="DiscordGuildWrapper"/>, not used for scripting
        /// </summary>
        public DiscordGuildWrapper(DiscordGuild guild, DiscordHandler client) : base(guild, client)
        {
        }

        /// <summary>
        /// Gets a specific channel from this guild
        /// </summary>
        /// <param name="id">Discord Channel Id</param>
        /// <returns>Null - Not Found, otherwise discord channel</returns>
        public DiscordChannelWrapper GetChannel(string id)
        {
            if (!ulong.TryParse(id, out ulong chid))
                return null;

            DiscordChannel channel = _client.GetChannelAsync(chid).ConfigureAwait(false).GetAwaiter().GetResult();

            if (channel == null)
                return null;

            return new DiscordChannelWrapper(channel, _client);
        }
    }

    /// <summary>
    /// Discord Member
    /// </summary>
    public sealed class DiscordMemberWrapper : DiscordWrapper<DiscordMember>
    {
        /// <summary>
        /// Discord Member ID
        /// </summary>
        public string ID => _value?.Id.ToString(CultureInfo.CurrentCulture) ?? "0";

        /// <summary>
        /// Creates a <see cref="DiscordMemberWrapper"/>, not used for scripting
        /// </summary>
        public DiscordMemberWrapper(DiscordMember member, DiscordHandler handler) : base(member, handler)
        {

        }

        /// <summary>
        /// Sends a message to the member
        /// </summary>
        /// <param name="message">Message to send</param>
        public void SendMessage(string message)
        {
            _value.SendMessageAsync(message).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Gets the verification status
        /// </summary>
        /// <returns>True - Verified, False - Not Verified</returns>
        public bool GetVerificationStatus()
        {
            using DBContext c = new DBContext();
            User user = c.User.FirstOrDefault(u => u.DiscordUserId == (long)_value.Id);

            if (user == null)
                return false;

            return true;
        }
    }
}
