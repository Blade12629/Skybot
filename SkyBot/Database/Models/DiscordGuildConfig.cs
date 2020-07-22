﻿using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SkyBot.Database.Models
{
    public class DiscordGuildConfig : IEquatable<DiscordGuildConfig>
    {
        public long Id { get; set; }
        public long GuildId { get; set; }

        public long AnalyzeChannelId { get; set; }
        public short AnalyzeWarmupMatches { get; set; }


        public long CommandChannelId { get; set; }

        public bool VerifiedNameAutoSet { get; set; }
        public long VerifiedRoleId { get; set; }

        public DiscordGuildConfig(long guildId, long analyzeChannelId, long commandChannelId, 
                                  bool verifiedNameAutoSet, long verifiedRoleId, short analyzeWarmupMatches)
        {
            GuildId = guildId;
            AnalyzeChannelId = analyzeChannelId;
            CommandChannelId = commandChannelId;
            VerifiedNameAutoSet = verifiedNameAutoSet;
            VerifiedRoleId = verifiedRoleId;
            AnalyzeWarmupMatches = analyzeWarmupMatches;
        }

        public DiscordGuildConfig()
        {
        }

        public bool TrySetValue(string key, string value)
        {
            try
            {
                PropertyInfo prop = typeof(DiscordGuildConfig).GetProperty(key, BindingFlags.Public);
                prop.SetValue(this, Convert.ChangeType(key, prop.PropertyType));
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public string TryGetValue(string key)
        {
            try
            {
                PropertyInfo prop = typeof(DiscordGuildConfig).GetProperty(key, BindingFlags.Public);

                return prop.GetValue(this)?.ToString() ?? "Not found";
            }
            catch (Exception)
            {
                return "Not found";
            }
        }

        public List<string> GetKeys()
        {
            return typeof(DiscordGuildConfig).GetProperties().Select(s => s.Name).ToList();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as DiscordGuildConfig);
        }

        public bool Equals([AllowNull] DiscordGuildConfig other)
        {
            return other != null &&
                   Id == other.Id &&
                   GuildId == other.GuildId;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, GuildId);
        }

        public static bool operator ==(DiscordGuildConfig left, DiscordGuildConfig right)
        {
            return EqualityComparer<DiscordGuildConfig>.Default.Equals(left, right);
        }

        public static bool operator !=(DiscordGuildConfig left, DiscordGuildConfig right)
        {
            return !(left == right);
        }
    }
}
