using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class SyncRolesCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => ResourcesCommands.SyncRolesCommand;

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.Public;

        public string Description => ResourcesCommands.SyncRolesCommandDescription;

        public string Usage => ResourcesCommands.SyncRolesCommandUsage;

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            using DBContext c = new DBContext();
            DiscordGuildConfig dgc = c.DiscordGuildConfig.FirstOrDefault(dgc => dgc.GuildId == (long)args.Guild.Id);

            if (dgc == null)
            {
                args.Channel.SendMessageAsync(ResourcesCommands.SyncRolesCommandConfigNotSetup);
                return;
            }

            User u = c.User.FirstOrDefault(u => u.DiscordUserId == (long)args.User.Id);

            if (u == null)
            {
                args.Channel.SendMessageAsync(ResourcesCommands.SyncRolesCommandVerifySelfFirst);
                return;
            }

            if (dgc.VerifiedNameAutoSet)
            {
                string username = SkyBot.Osu.API.V1.OsuApi.GetUserName((int)u.OsuUserId).Result;

                if (args.Member.Nickname == null || !args.Member.Nickname.Equals(username, StringComparison.CurrentCultureIgnoreCase))
                    args.Member.ModifyAsync(username, reason: "synchronized roles").Wait();
            }

            if (dgc.VerifiedRoleId > 0)
            {
                var drole = args.Guild.GetRole((ulong)dgc.VerifiedRoleId);

                if (!args.Member.Roles.Contains(drole))
                    args.Member.GrantRoleAsync(drole, "!syncroles").Wait();
            }

            args.Channel.SendMessageAsync($"{ResourcesCommands.SyncRolesCommandSyncSuccess} {args.User.Mention}");
        }
    }
}
