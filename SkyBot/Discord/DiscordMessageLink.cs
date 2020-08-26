using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace SkyBot.Discord
{
    public class DiscordMessageLink : IEquatable<DiscordMessageLink>
    {
        public ulong DiscordGuildId { get; }
        public ulong DiscordChannelId { get; }
        public ulong DiscordMessageId { get; set; }

        public DiscordMessageLink(ulong discordGuildId, ulong discordChannelId, ulong discordMessageId)
        {
            DiscordGuildId = discordGuildId;
            DiscordChannelId = discordChannelId;
            DiscordMessageId = discordMessageId;
        }

        public DiscordMessageLink(ulong discordChannelId, ulong discordMessageId)
        {
            DiscordChannelId = discordChannelId;
            DiscordMessageId = discordMessageId;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DiscordMessageLink);
        }

        public bool Equals([AllowNull] DiscordMessageLink other)
        {
            return other != null &&
                   DiscordGuildId == other.DiscordGuildId &&
                   DiscordChannelId == other.DiscordChannelId &&
                   DiscordMessageId == other.DiscordMessageId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(DiscordGuildId, DiscordChannelId, DiscordMessageId);
        }

        public static bool operator ==(DiscordMessageLink left, DiscordMessageLink right)
        {
            return EqualityComparer<DiscordMessageLink>.Default.Equals(left, right);
        }

        public static bool operator !=(DiscordMessageLink left, DiscordMessageLink right)
        {
            return !(left == right);
        }
    }
}
