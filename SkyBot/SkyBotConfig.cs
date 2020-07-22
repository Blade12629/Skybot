using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SkyBot
{
    public static class SkyBotConfig
    {
        public static string DiscordToken { get; set; }

        /// <summary>
        /// Set to true to use MariaDB otherwise use default MySQL
        /// </summary>
        public static bool UseMySQLMariaDB { get; set; }
        /// <summary>
        /// MySQL connection string, should contain "TreatTinyAsBoolean=true;"
        /// </summary>
        public static string MySQLConnectionString { get; set; }

        public static string IrcHost { get; set; }
        public static int IrcPort { get; set; }
        public static string IrcUser { get; set; }
        public static string IrcPass { get; set; }
        public static int IrcRateLimitResetDelayMS { get; set; }
        public static int IrcRateLimitMax { get; set; }

        public static int OsuApiRateLimitResetDelayMS { get; set; }
        public static int OsuApiRateLimitMax { get; set; }
        public static string OsuApiKey { get; set; }

        /// <summary>
        /// Loads the config from \typeName.cfg
        /// </summary>
        /// <returns>File read</returns>
        public static bool Read()
        {
            string file = typeof(SkyBotConfig).Name + ".cfg";

            if (!File.Exists(file))
                return false;

            Dictionary<string, PropertyInfo> props = typeof(SkyBotConfig).GetProperties(BindingFlags.Static | BindingFlags.Public).ToDictionary(k => k.Name);

            using (StreamReader sreader = new StreamReader(typeof(SkyBotConfig).Name + ".cfg"))
            {
                while(!sreader.EndOfStream)
                {
                    string line = sreader.ReadLine();
                    int propIndexEnd = line.IndexOf('=');

                    string propName = line.Substring(0, propIndexEnd);

                    if (!props.ContainsKey(propName))
                        continue;

                    line = line.Remove(0, propIndexEnd + 1);
                    PropertyInfo prop = props[propName];

                    prop.SetValue(null, Convert.ChangeType(line, prop.PropertyType));
                }
            }

            return true;
        }

        /// <summary>
        /// Writes the config to \typeName.cfg
        /// </summary>
        public static void Write()
        {
            Dictionary<string, PropertyInfo> props = typeof(SkyBotConfig).GetProperties(BindingFlags.Static | BindingFlags.Public).ToDictionary(k => k.Name);

            using (StreamWriter swriter = new StreamWriter(typeof(SkyBotConfig).Name + ".cfg"))
            {
                foreach (var pair in props)
                    swriter.WriteLine($"{pair.Key}={(pair.Value.GetValue(null)?.ToString() ?? "null")}");
            }
        }
    }
}
