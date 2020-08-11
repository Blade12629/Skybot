using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord;
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
            if (args.Parameters.Count > 0 && args.Guild != null && args.AccessLevel >= AccessLevel.Moderator)
            {
                ulong mentionId;
                if ((mentionId = DiscordHandler.ExtractMentionId(args.ParameterString)) > 0)
                {
                    if (System.Threading.Tasks.Task.Run(async () => await VerificationManager.SynchronizeVerification(mentionId, args.Guild.Id, args.Config).ConfigureAwait(false)).ConfigureAwait(false).GetAwaiter().GetResult())
                        args.Channel.SendMessageAsync($"{ResourcesCommands.SyncCommandSyncSuccess} {args.ParameterString}");
                    else
                        args.Channel.SendMessageAsync($"Failed to synchronize " + args.ParameterString);
                }
                else
                    args.Channel.SendMessageAsync($"Failed to parse mention " + args.ParameterString);

                return;
            }

            System.Threading.Tasks.Task.Run(async () => await VerificationManager.SynchronizeVerification((ulong)args.User.Id).ConfigureAwait(false)).ConfigureAwait(false).GetAwaiter().GetResult();


            args.Channel.SendMessageAsync($"{ResourcesCommands.SyncCommandSyncSuccess} {args.User.Mention}");
        }
    }
}
