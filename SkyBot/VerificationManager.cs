﻿using DSharpPlus.Entities;
using SkyBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SkyBot
{
    public static class VerificationManager
    {
        public static void StartVerification(DiscordUser user)
        {
            if (user == null)
                throw new ArgumentNullException(nameof(user));

            using DBContext c = new DBContext();
            var dmChannel = Program.DiscordHandler.Client.CreateDmAsync(user).Result;

            var dbuser = c.User.FirstOrDefault(u => u.DiscordUserId == (long)user.Id);

            if (dbuser != null)
            {
                dmChannel.SendMessageAsync("You are already verified");
                return;
            }

            Verification ver = c.Verification.FirstOrDefault(v => v.DiscordUserId == (long)user.Id);

            if (ver != null)
            {
                dmChannel.SendMessageAsync($"Your verification is already running, please send the following code to {Program.BotMention}: {ver.VerificationCode}").Wait();
                return;
            }

            string code = GenerateVerificationCode();
            while (c.Verification.FirstOrDefault(v => v.VerificationCode.Equals(code, StringComparison.CurrentCulture)) != null)
                code = GenerateVerificationCode();

            ver = new Verification((long)user.Id, code);
            c.Verification.Add(ver);
            c.SaveChanges();

            dmChannel.SendMessageAsync($"Started your verification, please send the following code to {Program.BotMention}: {ver.VerificationCode}").Wait();
        }

        private static string GenerateVerificationCode()
        {
            string code = "";

            for (int i = 0; i < 8; i++)
                code += Program.Random.Next(0, 9);

            return code;
        }

        public static void FinishVerification(string code, string osuUserName)
        {
            using DBContext c = new DBContext();
            
            Verification ver = c.Verification.FirstOrDefault(v => v.VerificationCode.Equals(code, StringComparison.CurrentCulture));

            if (ver == null)
            {
                Program.IRC.SendMessage(osuUserName, Resources.VerCodeInvalidNotFound);
                return;
            }

            var userJson = Osu.API.V1.OsuApi.GetUser(osuUserName, type: "name").Result;

            if (userJson == null)
            {
                Program.IRC.SendMessage(osuUserName, Resources.FailedFetchOsuApi);
                return;
            }

            User u = new User(ver.DiscordUserId, userJson.UserId);
            c.Verification.Remove(ver);
            c.User.Add(u);

            c.SaveChanges();

            System.Threading.Tasks.Task.Run(() => SendConfirmation(osuUserName, (ulong)ver.DiscordUserId));
        }

        private static void SendConfirmation(string osuUserName, ulong discordUserId)
        {
            Program.IRC.SendMessage(osuUserName, Resources.VerSuccess);

            var user = Program.DiscordHandler.Client.GetUserAsync(discordUserId).Result;

            if (user == null)
                return;

            var dmChannel = Program.DiscordHandler.Client.CreateDmAsync(user).Result;

            if (dmChannel == null)
                return;

            dmChannel.SendMessageAsync(Resources.VerSuccess).Wait();
        }
    }
}
