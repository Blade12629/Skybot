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
                Logger.Log(ex);
                return false;
            }
        }

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
