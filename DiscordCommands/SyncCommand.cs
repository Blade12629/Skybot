using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class SyncCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => ResourcesCommands.SyncCommand;

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        public string Description => ResourcesCommands.SyncCommandDescription;

        public string Usage => ResourcesCommands.SyncCommandUsage;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            using DBContext c = new DBContext();
            List<DiscordGuildConfig> dgcs = c.DiscordGuildConfig.ToList();

            for (int i = 0; i < dgcs.Count; i++)
            {
                try
                {
                    DiscordGuildConfig dgc = dgcs[i];

                    if (dgc == null)
                    {
                        args.Channel.SendMessageAsync(ResourcesCommands.SyncCommandConfigNotSetup);
                        return;
                    }

                    User u = c.User.FirstOrDefault(u => u.DiscordUserId == (long)args.User.Id);

                    if (u == null)
                    {
                        args.Channel.SendMessageAsync(ResourcesCommands.SyncCommandVerifySelfFirst);
                        return;
                    }

                    var dguild = Program.DiscordHandler.Client.GetGuildAsync((ulong)dgc.GuildId).ConfigureAwait(false).GetAwaiter().GetResult();
                    var dmember = dguild.GetMemberAsync(args.User.Id).ConfigureAwait(false).GetAwaiter().GetResult();

                    if (dgc.VerifiedNameAutoSet)
                    {
                        string username = SkyBot.Osu.API.V1.OsuApi.GetUserName((int)u.OsuUserId).Result;

                        dmember.ModifyAsync(username, reason: "synchronized name").Wait();
                    }

                    if (dgc.VerifiedRoleId > 0)
                    {
                        var drole = dguild.GetRole((ulong)dgc.VerifiedRoleId);

                        if (!dmember.Roles.Contains(drole))
                            dmember.GrantRoleAsync(drole, "synchronized role").Wait();
                    }

                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
                {
                    Logger.Log(ex, LogLevel.Error);
                }
            }

            args.Channel.SendMessageAsync($"{ResourcesCommands.SyncCommandSyncSuccess} {args.User.Mention}");
        }
    }
}
