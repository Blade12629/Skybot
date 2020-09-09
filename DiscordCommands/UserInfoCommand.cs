using DSharpPlus.Entities;
using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class UserInfoCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => "userinfo";

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.None;

        public string Description => "Shows info about a user";

        public string Usage => "!userinfo <discordId/mention>";
        public bool AllowOverwritingAccessLevel => false;

        public int MinParameters => 1;

        public void Invoke(DiscordHandler client, CommandHandler handler, CommandEventArg args)
        {
            args.Parameters[0] = args.Parameters[0].Trim('<', '>', '@', '!');

            DiscordUser user = null;
            if (ulong.TryParse(args.Parameters[0], out ulong uid))
            {
                try
                {
                    user = client.GetUserAsync(uid).ConfigureAwait(false).GetAwaiter().GetResult();
                }
                catch (Exception)
                {

                }
            }
            
            if (user == null)
            {
                args.Channel.SendMessageAsync($"User {args.Parameters[0]} not found");
                return;
            }

            DiscordEmbedBuilder builder = new DiscordEmbedBuilder()
            {
                Title = $"Userinfo for {user.Username} ({user.Id})",
                ThumbnailUrl = user.AvatarUrl,
                Timestamp = DateTime.UtcNow
            };

            using DBContext c = new DBContext();
            User u = c.User.FirstOrDefault(u => u.DiscordUserId == (long)uid);

            if (u != null)
                builder.AddField("Osu UserId", u.OsuUserId.ToString(CultureInfo.CurrentCulture));

            if (args.Guild != null)
            {
                try
                {
                    DiscordMember dmember = args.Guild.GetMemberAsync(uid).ConfigureAwait(false).GetAwaiter().GetResult();

                    builder.AddField("Join Date", dmember.JoinedAt.ToString(CultureInfo.CurrentCulture));
                }
                catch (Exception)
                {

                }
            }

            builder.AddField("Created On", user.CreationTimestamp.UtcDateTime.ToString(CultureInfo.CurrentCulture));

            builder.AddField("Verified", (u != null).ToString(CultureInfo.CurrentCulture));

            args.Channel.SendMessageAsync(embed: builder.Build());
        }
    }
}
