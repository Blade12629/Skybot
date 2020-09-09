using SkyBot.Database.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.IO;
using System.Linq;

namespace SkyBot.API
{
    public static class APIAuth
    {
        /// <summary>
        /// Checks if a api key is valid
        /// </summary>
        public static bool CheckApiKey(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return false;

                string hashedKey = HashKey(key);

                using DBContext c = new DBContext();
                APIUser user = c.APIUser.FirstOrDefault(u => u.APIKeyMD5.Equals(hashedKey, StringComparison.CurrentCulture));

                if (user == null ||
                    !user.IsValid)
                    return false;

                return true;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
            {
                Logger.Log(ex, LogLevel.Error);
                return false;
            }
        }

        public static AccessLevel GetApiKeyAccess(string key)
        {
            try
            {
                if (string.IsNullOrEmpty(key))
                    return AccessLevel.User;

                string hashedKey = HashKey(key);

                using DBContext c = new DBContext();
                APIUser user = c.APIUser.FirstOrDefault(u => u.APIKeyMD5.Equals(hashedKey, StringComparison.CurrentCulture));

                if (user == null ||
                    !user.IsValid)
                    return AccessLevel.User;

                Permission perm = c.Permission.FirstOrDefault(p => p.DiscordUserId == user.DiscordUserId && 
                                                                  (p.AccessLevel == (int)AccessLevel.Dev || p.DiscordGuildId == 0));

                if (perm == null)
                    return AccessLevel.User;

                return (AccessLevel)perm.AccessLevel;
            }
            catch (Exception ex)
            {
                Logger.Log(ex, LogLevel.Error);
                return AccessLevel.User;
            }
        }

        /// <summary>
        /// Hashes a string and returns it's hashed base64 representation
        /// </summary>
        public static string HashKey(string key)
        {
            using (SHA512 sha = SHA512.Create())
            {
                byte[] keyBytes = Encoding.UTF8.GetBytes(key);
                byte[] hash = sha.ComputeHash(keyBytes);

                return Convert.ToBase64String(hash);
            }
        }
    }
}
