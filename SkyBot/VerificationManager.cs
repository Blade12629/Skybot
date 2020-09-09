﻿using DSharpPlus.Entities;
using SkyBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot
{
    /// <summary>
    /// Handles the bots verification system
    /// </summary>
    public static class VerificationManager
    {
        /// <summary>
        /// Starts the verification for a specific user
        /// </summary>
        /// <param name="user"></param>
        public static void StartVerification(DiscordUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using DBContext c = new DBContext();
            var dmChannel = Program.DiscordHandler.GetDmChannelAsync(user).Result;

            var dbuser = c.User.FirstOrDefault(u => u.DiscordUserId == (long)user.Id);

            if (dbuser != null)
            {
                dmChannel.SendMessageAsync("You are already verified");
                return;
            }

            Verification ver = c.Verification.FirstOrDefault(v => v.DiscordUserId == (long)user.Id);

            if (ver != null)
            {
                dmChannel.SendMessageAsync($"Your verification is already running, please send the following to {Program.IRC.Nick} in osu via pm: !verify: {ver.VerificationCode}").Wait();
                return;
            }

            string code = GenerateVerificationCode();
            while (c.Verification.FirstOrDefault(v => v.VerificationCode.Equals(code, StringComparison.CurrentCulture)) != null)
                code = GenerateVerificationCode();

            ver = new Verification((long)user.Id, code);
            c.Verification.Add(ver);
            c.SaveChanges();

            dmChannel.SendMessageAsync($"Started your verification, please send the following to {Program.IRC.Nick} in osu via pm: !verify {ver.VerificationCode}").Wait();
        }

        /// <summary>
        /// Generates a 9 character long verification code out of numbers
        /// </summary>
        private static string GenerateVerificationCode()
        {
            StringBuilder codeBuilder = new StringBuilder();

            for (int i = 0; i < 8; i++)
                codeBuilder.Append(Program.Random.Next(0, 9));

            return codeBuilder.ToString();
        }

        /// <summary>
        /// Synchronizes a user for every discord guild he currently is in
        /// </summary>
        /// <returns>User found</returns>
        public static async Task<bool> SynchronizeVerification(ulong discordUserId)
        {
            using DBContext c = new DBContext();
            List<DiscordGuildConfig> cfgs = c.DiscordGuildConfig.ToList();
            User u = c.User.FirstOrDefault(u => u.DiscordUserId == (long)discordUserId);

            if (u == null)
                return false;

            foreach (DiscordGuildConfig dgc in cfgs)
                await SynchronizeVerification(discordUserId, (int)u.OsuUserId, (ulong)dgc.GuildId, (ulong)dgc.VerifiedRoleId, dgc.VerifiedNameAutoSet).ConfigureAwait(false);

            return true;
        }

        /// <summary>
        /// Synchronizes a user for a specific discord guild
        /// </summary>
        /// <returns>Verification success</returns>
        public static async Task<bool> SynchronizeVerification(ulong discordUserId, ulong discordGuildId, DiscordGuildConfig config = null)
        {
            using DBContext c = new DBContext();
            DiscordGuildConfig dgc;

            if (config == null)
                dgc = c.DiscordGuildConfig.FirstOrDefault(dgc => dgc.GuildId == (long)discordGuildId);
            else
                dgc = config;

            if (dgc == null)
                return false;

            User u = c.User.FirstOrDefault(u => u.DiscordUserId == (long)discordUserId);

            if (u == null)
                return false;


            return await SynchronizeVerification(discordUserId, (int)u.OsuUserId, discordGuildId, (ulong)dgc.VerifiedRoleId, dgc.VerifiedNameAutoSet).ConfigureAwait(false);
        }

        /// <summary>
        /// Synchronizes a specific user
        /// </summary>
        /// <param name="verifiedRoleId">Verification role, 0 = don't set</param>
        /// <param name="verifiedNameAutoSet">Set the osu username as new discord name</param>
        public static async Task<bool> SynchronizeVerification(ulong discordUserId, int osuUserId, ulong discordGuildId, ulong verifiedRoleId, bool verifiedNameAutoSet)
        {
            DiscordGuild guild;
            DiscordMember member;
            try
            {
                guild = await Program.DiscordHandler.GetGuildAsync(discordGuildId).ConfigureAwait(false);
                member = await guild.GetMemberAsync(discordUserId).ConfigureAwait(false);

                if (verifiedRoleId > 0)
                {
                    DiscordRole role = guild.GetRole(verifiedRoleId);

                    if (!member.Roles.Contains(role))
                        await member.GrantRoleAsync(role, "synchronized").ConfigureAwait(false);
                }

                if (verifiedNameAutoSet)
                {
                    string username = Osu.API.V1.OsuApi.GetUserName(osuUserId).Result;

                    member.ModifyAsync(username, reason: "synchronized name").Wait();
                }
            }
            catch(DSharpPlus.Exceptions.UnauthorizedException)
            {

            }
            catch (DSharpPlus.Exceptions.NotFoundException)
            {

            }

            return true;
        }

        /// <summary>
        /// Completes a verification
        /// </summary>
        /// <param name="code">Verification code</param>
        /// <param name="osuUserName"></param>
        public static void FinishVerification(string code, string osuUserName)
        {
            using DBContext c = new DBContext();
            
            Verification ver = c.Verification.FirstOrDefault(v => v.VerificationCode.Equals(code, StringComparison.CurrentCulture));

            if (ver == null)
            {
                Program.IRC.SendMessageAsync(osuUserName, Resources.VerCodeInvalidNotFound).ConfigureAwait(false);
                return;
            }

            var userJson = Osu.API.V1.OsuApi.GetUser(osuUserName, type: "name").Result;

            if (userJson == null)
            {
                Program.IRC.SendMessageAsync(osuUserName, Resources.FailedFetchOsuApi).ConfigureAwait(false);
                return;
            }

            User u = c.User.FirstOrDefault(u => u.OsuUserId == userJson.UserId);

            if (u != null)
            {
                Task.Run(() => SendUserAlreadyExists(osuUserName, (ulong)ver.DiscordUserId));
                return;
            }

            u = new User(ver.DiscordUserId, userJson.UserId);
            c.Verification.Remove(ver);
            c.User.Add(u);

            c.SaveChanges();

            Task.Run(async () => await SynchronizeVerification((ulong)u.DiscordUserId).ConfigureAwait(false)).ConfigureAwait(false).GetAwaiter().GetResult();

            Task.Run(() => SendConfirmation(osuUserName, (ulong)ver.DiscordUserId));
        }

        /// <summary>
        /// Sends the verification confirmation to both the irc user and discord user
        /// </summary>
        private static void SendConfirmation(string osuUserName, ulong discordUserId)
        {
            try
            {
                Program.IRC.SendMessageAsync(osuUserName, Resources.VerSuccess).ConfigureAwait(false);
            }
            catch (Exception)
            {
                //just skip if we can't message or find the user
            }

            var user = Program.DiscordHandler.GetUserAsync(discordUserId).Result;

            if (user == null)
                return;

            var dmChannel = Program.DiscordHandler.GetDmChannelAsync(user).Result;

            if (dmChannel == null)
                return;

            dmChannel.SendMessageAsync(Resources.VerSuccess).Wait();
        }

        private static void SendUserAlreadyExists(string osuUserName, ulong discordUserId)
        {
            try
            {
                Program.IRC.SendMessageAsync(osuUserName, Resources.VerUserAlreadyExists).ConfigureAwait(false);
            }
            catch (Exception)
            {
                //just skip if we can't message or find the user
            }

            var user = Program.DiscordHandler.GetUserAsync(discordUserId).Result;

            if (user == null)
                return;

            var dmChannel = Program.DiscordHandler.GetDmChannelAsync(user).Result;

            if (dmChannel == null)
                return;

            dmChannel.SendMessageAsync(Resources.VerUserAlreadyExists).Wait();
        }
    }
}
