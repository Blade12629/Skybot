using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace DiscordCommands
{
    public class MuteCommand : ICommand, IDisposable
    {
        public bool IsDisabled { get; set; }

        public bool IsDisposed { get; private set; }

        public string Command => "mute";

        public AccessLevel AccessLevel => AccessLevel.Moderator;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Mutes or unmutes someone";

        public string Usage => "!mute <mention/id> <duration in minutes> <reason>";

        public int MinParameters => 2;

        private Timer _muteTimer;

        public MuteCommand()
        {
            _muteTimer = new Timer(10000)
            {
                AutoReset = true,
            };
            _muteTimer.Elapsed += OnMuteTimerElapsed;

            System.Threading.Tasks.Task.Run(() =>
            {
                System.Threading.Tasks.Task.Delay(2500).Wait();
                _muteTimer.Start();
            });
        }

        ~MuteCommand()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (IsDisposed)
                return;

            _muteTimer?.Stop();
            _muteTimer?.Dispose();

            IsDisposed = true;
        }

        private void OnMuteTimerElapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
            using DBContext c = new DBContext();
            List<Mute> mutes = c.Mute.Where(m => m.StartTime.AddMinutes(m.DurationM) <= DateTime.UtcNow && !m.Unmuted).ToList();

            if (mutes.Count == 0)
                return;

            for (int i = 0; i < mutes.Count; i++)
            {
                Mute m = mutes[i];

                try
                {
                    var guild = Program.DiscordHandler.Client.GetGuildAsync((ulong)m.DiscordGuildId).ConfigureAwait(false).GetAwaiter().GetResult();
                    var member = guild.GetMemberAsync((ulong)m.DiscordUserId).ConfigureAwait(false).GetAwaiter().GetResult();

                    DiscordGuildConfig dgc = c.DiscordGuildConfig.FirstOrDefault(dgc => dgc.GuildId == (long)guild.Id);
                    if (dgc == null)
                        continue;

                    var role = guild.GetRole((ulong)dgc.MutedRoleId);

                    if (member.Roles.Contains(role))
                        member.RevokeRoleAsync(role, "unmuted");

                    m.Unmuted = true;
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    Logger.Log("Exception at unmuting, " + ex, LogLevel.Error);
                }
            }

            c.Mute.UpdateRange(mutes);
            c.SaveChanges();

            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Logger.Log(ex, LogLevel.Error);
            }
        }

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            using DBContext c = new DBContext();
            DiscordGuildConfig dgc = args.Config;

            if (dgc == null || dgc.MutedRoleId == 0)
            {
                args.Channel.SendMessageAsync("You need to setup your muted role first in the config");
                return;
            }

            args.Parameters[0] = args.Parameters[0].Trim('<', '@', '!', '>');

            DiscordMember member = null;
            if (ulong.TryParse(args.Parameters[0], out ulong uid))
            {
                try
                {
                    member = args.Guild.GetMemberAsync(uid).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (DSharpPlus.Exceptions.NotFoundException)
                {

                }
            }

            if (member == null)
            {
                HelpCommand.ShowHelp(args.Channel, this, "Could not find member or parse id: " + args.Parameters[0]);
                return;
            }

            if (!long.TryParse(args.Parameters[1], out long durationM))
            {
                HelpCommand.ShowHelp(args.Channel, this, "Failed to parse duration: " + args.Parameters[1]);
                return;
            }

            var drole = args.Guild.GetRole((ulong)dgc.MutedRoleId);
            Mute m;
            if (member.Roles.Contains(drole))
            {
                //Update duration if diffrent
                m = c.Mute.FirstOrDefault(mu => mu.DiscordGuildId == (long)args.Guild.Id && mu.DiscordUserId == (long)uid && !mu.Unmuted);

                if (m != null)
                {
                    if (m.DurationM == durationM)
                    {
                        args.Channel.SendMessageAsync("Not updated, time is the same");
                        return;
                    }

                    m.DurationM = durationM;
                    c.Mute.Update(m);
                    c.SaveChanges();

                    args.Channel.SendMessageAsync("Updated duration");

                    return;
                }
                else
                    member.RevokeRoleAsync(drole, "invalid mute, revoking").ConfigureAwait(false).GetAwaiter().GetResult();
            }

            if (args.Parameters.Count < 3)
            {
                HelpCommand.ShowHelp(args.Channel, this);
                return;
            }

            StringBuilder reasonBuilder = new StringBuilder(args.Parameters[2]);

            for (int i = 3; i < args.Parameters.Count; i++)
                reasonBuilder.Append(" " + args.Parameters[i]);

            m = new Mute((long)uid, (long)args.Guild.Id, DateTime.UtcNow, durationM, args.ParameterString.Remove(0, args.Parameters[0].Length + args.Parameters[1].Length + 2));

            member.GrantRoleAsync(drole, $"muted by {args.User.Username} for {durationM} minutes, reason: {m.Reason}");

            c.Mute.Add(m);
            c.SaveChanges();

            args.Channel.SendMessageAsync($"Muted {member.Username} for {durationM} minutes, reason: {m.Reason}");
        }
    }
}
