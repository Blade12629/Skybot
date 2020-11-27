﻿using DSharpPlus.Entities;
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

        public string Command => "sync";

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        public string Description => "Synchronizes you/Force synchronize someone/synchronize all users";

        public string Usage =>  "{prefix}sync\n\n" +
                                "Moderator:\n" +
                                "{prefix}sync [discordUserId/Mention]\n\n" +
                                "Host:\n" +
                                "{prefix}sync @@all\n";


        public bool AllowOverwritingAccessLevel => false;

        public int MinParameters => 0;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            if (args.Parameters.Count > 0 && args.Guild != null && args.AccessLevel >= AccessLevel.Moderator)
            {
                if (args.AccessLevel >= AccessLevel.Host && 
                    args.Parameters[0].Equals("@@all", StringComparison.CurrentCultureIgnoreCase))
                {

                    args.Channel.SendMessageAsync("Started synchronizing all users").ConfigureAwait(false).GetAwaiter().GetResult();

                    int errors = 0;
                    StringBuilder strb = new StringBuilder();

                    foreach (var member in args.Guild.GetAllMembersAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                    {
                        if (!VerificationManager.SynchronizeVerification(member.Id, args.Guild.Id, args.Config).ConfigureAwait(false).GetAwaiter().GetResult())
                        {
                            strb.Append($"{member.Id} ");
                            errors++;
                        }
                    }

                    if (errors == 0)
                        args.Channel.SendMessageAsync("Synchronized all users");
                    else
                    {
                        if (strb.Length > 1990)
                            strb.Length = 1990;

                        args.Channel.SendMessageAsync($"Synchronized users, failed to synchronize {errors} users:");
                        args.Channel.SendMessageAsync($"```\n{strb.ToString()}\n```");
                    }


                    return;
                }

                ulong mentionId;
                if ((mentionId = DiscordHandler.ExtractMentionId(args.ParameterString)) > 0)
                {
                    if (System.Threading.Tasks.Task.Run(async () => await VerificationManager.SynchronizeVerification(mentionId, args.Guild.Id, args.Config).ConfigureAwait(false)).ConfigureAwait(false).GetAwaiter().GetResult())
                        args.Channel.SendMessageAsync($"Synchronized {args.ParameterString}");
                    else
                        args.Channel.SendMessageAsync($"Failed to synchronize " + args.ParameterString);
                }
                else
                    args.Channel.SendMessageAsync($"Failed to parse mention " + args.ParameterString);

                return;
            }

            System.Threading.Tasks.Task.Run(async () => await VerificationManager.SynchronizeVerification((ulong)args.User.Id).ConfigureAwait(false)).ConfigureAwait(false).GetAwaiter().GetResult();


            args.Channel.SendMessageAsync($"Synchronized {args.User.Mention}");
        }
    }
}
