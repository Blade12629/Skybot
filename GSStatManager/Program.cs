using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace GSStatManager
{
    class Program
    {
        private static HTTPClient _client;


        static void Main(string[] args)
            => MainTask(args).ConfigureAwait(false).GetAwaiter().GetResult();

        static async Task MainTask(string[] args)
        {
            try
            {
                Config cfg = Config.Load();

                _client = new HTTPClient(cfg.Host, cfg.Port, cfg.APIKey);

                while (true)
                {
                    string line = Console.ReadLine().TrimEnd(' ');

                    if (string.IsNullOrEmpty(line))
                        continue;

                    List<string> split = line.Split(' ').ToList();

                    if (split.Count == 0)
                        continue;

                    string cmd = split[0].ToLower(CultureInfo.CurrentCulture);
                    split.RemoveAt(0);
                    line = line.Remove(0, cmd.Length).TrimStart(' ');
                    
                    switch(cmd)
                    {
                        case "exit":
                        case "close":
                            Environment.Exit(0);
                            return;

                        case "clr":
                        case "clear":
                            Console.Clear();
                            continue;

                        case "submit":
                            Submit(line);
                            continue;

                        case "profile":
                        case "get":
                            if (!long.TryParse(split[0], out long osuId))
                                throw new ArgumentException("Failed to parse osuId");

                            if (split.Count > 1)
                                Get(osuId, line);
                            else
                                Get(osuId);
                            continue;

                        case "help":
                            Console.WriteLine("Commands:\nexit/close\nclr/clear\nsubmit <path>\nprofile/get osuId [path, default: profile.json]\n———————————————");
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            await Task.Delay(-1);
        }

        private static void Submit(string path = "tourneyData.tsv")
        {
            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    Console.WriteLine("Failed to submit, path is null or empty");
                    return;
                }

                SkyBot.API.Data.GlobalStatistics.GlobalStatsTournament tournament = SkyBot.API.Data.GlobalStatistics.GlobalStatsTournament.FromTSVFile(path);
                _client.SubmitTourneyStats(tournament);
                Console.WriteLine("Submitted");
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine($"WebException {ex.Status}: {ex.Message}");
            }
        }

        private static void Get(long osuId, string path = "profile.json")
        {
            try
            {
                var profile = _client.GetProfile(osuId);

                if (profile == null)
                    return;

                string json = Newtonsoft.Json.JsonConvert.SerializeObject(profile, Newtonsoft.Json.Formatting.Indented);

                File.WriteAllText(path, json);
                Console.WriteLine("Saved to " + path);
            }
            catch (System.Net.WebException ex)
            {
                Console.WriteLine($"WebException {ex.Status}: {ex.Message}");
            }
        }
    }
}
