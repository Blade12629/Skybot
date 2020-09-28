using SkyBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace DiscordCommands.Scripting.Wrappers
{
    /// <summary>
    /// Convert wrapper, contains method to help converting objects
    /// </summary>
    public class ConvertWrapper
    {
        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="int"/>
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, if failed returns default value: 0</returns>
        public int StrToInt(string value)
        {
            if (int.TryParse(value, out int r))
                return r;

            return 0;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="long"/>
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, if failed returns default value: 0</returns>
        public long StrToRealLong(string value)
        {
            if (long.TryParse(value, out long r))
                return r;

            return 0L;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="ulong"/>
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, if failed returns default value: 0</returns>
        public ulong StrToRealUlong(string value)
        {
            if (ulong.TryParse(value, out ulong r))
                return r;

            return 0UL;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="double"/>
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, if failed returns default value: 0</returns>
        public double StrToDouble(string value)
        {
            if (double.TryParse(value, out double r))
                return r;

            return 0.0;
        }

        /// <summary>
        /// Converts <paramref name="value"/> to <see cref="bool"/>
        /// </summary>
        /// <param name="value">Value to convert</param>
        /// <returns>Converted value, if failed returns default value: false</returns>
        public bool StrToBool(string value)
        {
            if (bool.TryParse(value, out bool r))
                return r;

            return false;
        }
    }

    /// <summary>
    /// Script wrapper, contains methods to help scripting
    /// </summary>
    public class ScriptWrapper
    {
        /// <summary>
        /// Waits for a specific amount of time
        /// </summary>
        /// <param name="duration">Wait time in milliseconds</param>
        public void Wait(int duration)
        {
            Task.Delay(duration).ConfigureAwait(false).GetAwaiter().GetResult();
        }
        /// <summary>
        /// Waits for a specific amount of time, equivalent of <see cref="Wait(int)"/>
        /// </summary>
        /// <param name="duration">Wait time in milliseconds</param>
        public void Pause(int duration)
        {
            Wait(duration);
        }
        /// <summary>
        /// Waits for a specific amount of time, equivalent of <see cref="Wait(int)"/>
        /// </summary>
        /// <param name="duration">Wait time in milliseconds</param>
        public void Timeout(int duration)
        {
            Wait(duration);
        }

        /// <summary>
        /// Throws an exception which also results in the script terminating
        /// </summary>
        /// <param name="exMsg">Exception message</param>
        public void Throw(string exMsg)
        {
            throw new Exception(exMsg);
        }
        /// <summary>
        /// Throws an exception which also results in the script terminating, equivalent of <see cref="Throw(string)"/>
        /// </summary>
        /// <param name="exMsg">Exception message</param>
        public void Error(string exMsg)
        {
            Throw(exMsg);
        }
        /// <summary>
        /// Throws an exception which also results in the script terminating, equivalent of <see cref="Throw(string)"/>
        /// </summary>
        /// <param name="exMsg">Exception message</param>
        public void Exception(string exMsg)
        {
            Throw(exMsg);
        }

        /// <summary>
        /// Exits the script
        /// </summary>
        public void Exit()
        {
            throw new ScriptExitException();
        }
        /// <summary>
        /// Exits the script, equivalent of <see cref="Exit"/>
        /// </summary>
        public void Abort()
        {
            Exit();
        }
        /// <summary>
        /// Exits the script, equivalent of <see cref="Exit"/>
        /// </summary>
        public void Cancel()
        {
            Exit();
        }
    }

    /// <summary>
    /// Guild wrapper, contains current servers config
    /// </summary>
    public class ConfigWrapper
    {
        /// <summary>
        /// Analyze Channel
        /// </summary>
        public long AnalyzeChannelId { get; set; }
        /// <summary>
        /// Analyze Warmup Match Count
        /// </summary>
        public short AnalyzeWarmupMatches { get; set; }

        /// <summary>
        /// Command Channel
        /// </summary>
        public long CommandChannelId { get; set; }

        /// <summary>
        /// Automatically set name if user is verified
        /// </summary>
        public bool VerifiedNameAutoSet { get; set; }
        /// <summary>
        /// Automatically set role if user is verified
        /// </summary>
        public long VerifiedRoleId { get; set; }

        /// <summary>
        /// Ticket Channel
        /// </summary>
        public long TicketDiscordChannelId { get; set; }

        /// <summary>
        /// Welcome message when a user joins the server, requires <see cref="WelcomeChannel"/>
        /// </summary>
        public string WelcomeMessage { get; set; }
        /// <summary>
        /// Channel to send welcome message to
        /// </summary>
        public long WelcomeChannel { get; set; }

        /// <summary>
        /// Muted Role
        /// </summary>
        public long MutedRoleId { get; set; }

        /// <summary>
        /// Command Prefix
        /// </summary>
        public char? Prefix { get; set; }

        /// <summary>
        /// Debug(/Notifications) enabled
        /// </summary>
        public bool Debug { get; set; }
        /// <summary>
        /// Debug(/Notifications) channel
        /// </summary>
        public long DebugChannel { get; set; }

        /// <summary>
        /// Blacklist role, can be the same as <see cref="MutedRoleId"/>
        /// </summary>
        public long BlacklistRoleId { get; set; }

        /// <summary>
        /// Creates the config wrapper, not useable for scripting
        /// </summary>
        /// <param name="cfg"></param>
        public ConfigWrapper(DiscordGuildConfig cfg)
        {
            if (cfg == null)
                return;

            AnalyzeChannelId = cfg.AnalyzeChannelId;
            AnalyzeWarmupMatches = cfg.AnalyzeWarmupMatches;
            CommandChannelId = cfg.CommandChannelId;
            VerifiedNameAutoSet = cfg.VerifiedNameAutoSet;
            VerifiedRoleId = cfg.VerifiedRoleId;
            TicketDiscordChannelId = cfg.TicketDiscordChannelId;
            WelcomeMessage = cfg.WelcomeMessage;
            MutedRoleId = cfg.MutedRoleId;
            Prefix = cfg.Prefix;
            Debug = cfg.Debug;
            DebugChannel = cfg.DebugChannel;
            BlacklistRoleId = cfg.BlacklistRoleId;
        }

        /// <summary>
        /// Sets a config, not useable for scripting
        /// </summary>
        /// <param name="cfg"></param>
        public void SetConfig(DiscordGuildConfig cfg)
        {
            if (cfg == null)
                throw new ArgumentNullException(nameof(cfg));

            cfg.AnalyzeChannelId = AnalyzeChannelId;
            cfg.AnalyzeWarmupMatches = AnalyzeWarmupMatches;
            cfg.CommandChannelId = CommandChannelId;
            cfg.VerifiedNameAutoSet = VerifiedNameAutoSet;
            cfg.VerifiedRoleId = VerifiedRoleId;
            cfg.TicketDiscordChannelId = TicketDiscordChannelId;
            cfg.WelcomeMessage = WelcomeMessage;
            cfg.MutedRoleId = MutedRoleId;
            cfg.Prefix = Prefix;
            cfg.Debug = Debug;
            cfg.DebugChannel = DebugChannel;
            cfg.BlacklistRoleId = BlacklistRoleId;
        }
    }
}
