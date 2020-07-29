using SkyBot.Osu.API.V1.Json;
using SkyBot.Ratelimits;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Osu.API.V1
{
    public static class OsuApi
    {
        private static string API_Key { get { return SkyBotConfig.OsuApiKey; } }
        private static string API_URL = "https://osu.ppy.sh/api/";
        private static QueueRateLimiter _qrl { get; } = new QueueRateLimiter(0, SkyBotConfig.OsuApiRateLimitMax, TimeSpan.FromMilliseconds(SkyBotConfig.OsuApiRateLimitResetDelayMS));

        // Parameters: k* api key, mp* matchid | * = required
        /// <summary>
        /// Gets a match from the api
        /// </summary>
        public static async Task<JsonGetMatch> GetMatch(int matchId)
        {
            if (matchId <= 0) 
                return null;

            return await GetJson<JsonGetMatch>(string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0}get_match?k={1}&mp={2}", API_URL, API_Key, matchId)).ConfigureAwait(false);
        }

        private static async Task<T> GetJson<T>(string url)
        {
            return await Task.Run(async () =>
            {
                JsonDataTransmitter<T> jdt = new JsonDataTransmitter<T>();

                bool result = _qrl.Increment(new Action(() =>
                {
                    string jsonInput = "";

                    using (WebClient webClient = new WebClient())
                    {
                        jsonInput = webClient.DownloadString(url);
                    }

                    jdt.Value = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonInput);
                }),
                    new Action<object>(o =>
                    {
                        JsonDataTransmitter<T> jdt = (JsonDataTransmitter<T>)o;
                        jdt.Status = true;

                    }),
                    jdt);

                while (jdt.Value == null)
                    await Task.Delay(5).ConfigureAwait(false);

                return jdt.Value;
            }).ConfigureAwait(false);
        }

        private class JsonDataTransmitter<T>
        {
            public bool Status { get; set; }
            public T Value { get; set; }
        }
        
        /// <summary>
        /// Gets a user from the api
        /// </summary>
        /// <param name="user">string username for <paramref name="type"/> "name" || int userid for <paramref name="type"/> "id"  </param>
        /// <param name="mode">0 = osu!, 1 = Taiko, 2 = CtB, 3 = osu!mania</param>
        /// <param name="type">name == string || id == int</param>
        /// <param name="event_days">1 - 31</param>
        /// <returns>user json</returns>
        public static async Task<JsonGetUser> GetUser(object user, int mode = 0, string type = "id", int eventDays = 1)
        {
            if (user == null)
                throw new ArgumentException(Resources.CannotBeNullEmptyException, nameof(user));
            if (string.IsNullOrEmpty(type))
                throw new ArgumentException(Resources.CannotBeNullEmptyException, nameof(type));

            if (type.Equals("id", StringComparison.CurrentCultureIgnoreCase) && !(user is int))
                throw new ArgumentException("string type is 'id'" + Environment.NewLine +
                                            " - object user should be int but is instead: " + nameof(user));
            else if (type.Equals("name", StringComparison.CurrentCultureIgnoreCase) && !(user is string))
                throw new ArgumentException("string type is 'name'" + Environment.NewLine +
                                            " - object user should be string but is instead: " + nameof(user));

            JsonGetUser[] users = await GetJson<JsonGetUser[]>(string.Format(System.Globalization.CultureInfo.CurrentCulture, 
                                                                             "{0}get_user?k={1}&u={2}&m={3}&t={4}&event_days={5}", API_URL, API_Key, user.ToString(), 
                                                                             mode.ToString(System.Globalization.CultureInfo.CurrentCulture), type, eventDays))
                                                               .ConfigureAwait(false);

            return users?[0] ?? null;
        }

        /// <summary>
        /// Gets a beatmap from the api
        /// </summary>
        public static async Task<JsonGetBeatmap> GetBeatMap(int beatmapId = 0, GameModeEnum mode = GameModeEnum.standard, int convertedMaps = 1, int limitSearchLimit = 30)
        {
            return (await GetJson<JsonGetBeatmap[]>(string.Format(System.Globalization.CultureInfo.CurrentCulture, "{0}get_beatmaps?k={1}&b={2}&m={3}&a={4}&limit={5}", API_URL, API_Key, beatmapId, (int)mode, convertedMaps, limitSearchLimit)).ConfigureAwait(false))?[0] ?? null;
        }

        public static async Task<string> GetUserName(int user)
            => (await GetUser(user).ConfigureAwait(false))?.UserName ?? "";

        /// <summary>
        /// writes a json to <see cref="T"/>
        /// </summary>
        private static T WriteJson<T>(string jsonInput) where T : class
        {
            return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonInput) as T;
        }

        [Flags]
        public enum Mods
        {
            None = 0,
            NF = 1,
            EZ = 2,
            TouchDevice = 4,
            HD = 8,
            HR = 16,
            SD = 32,
            DT = 64,
            RLX = 128,
            HT = 256,
            NC = 512, // Only set along with DoubleTime. i.e: NC only gives 576
            FL = 1024,
            AUTO = 2048,
            SO = 4096,
            AP = 8192,    // Autopilot
            PF = 16384, // Only set along with SuddenDeath. i.e: PF only gives 16416  
            Key4 = 32768,
            Key5 = 65536,
            Key6 = 131072,
            Key7 = 262144,
            Key8 = 524288,
            FadeIn = 1048576,
            Random = 2097152,
            Cinema = 4194304,
            Target = 8388608,
            Key9 = 16777216,
            KeyCoop = 33554432,
            Key1 = 67108864,
            Key3 = 134217728,
            Key2 = 268435456,
            ScoreV2 = 536870912,
            LastMod = 1073741824,
            KeyMod = Key1 | Key2 | Key3 | Key4 | Key5 | Key6 | Key7 | Key8 | Key9 | KeyCoop,
            FreeModAllowed = NF | EZ | HD | HR | SD | FL | FadeIn | RLX | AP | SO | KeyMod,
            ScoreIncreaseMods = HD | HR | DT | FL | FadeIn
        }
    }
}
