using DSharpPlus.Entities;
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
            System.Threading.Tasks.Task.Run(async () => await VerificationManager.SynchronizeVerification((ulong)args.User.Id).ConfigureAwait(false)).ConfigureAwait(false).GetAwaiter().GetResult();


            args.Channel.SendMessageAsync($"{ResourcesCommands.SyncCommandSyncSuccess} {args.User.Mention}");
        }
    }
}
