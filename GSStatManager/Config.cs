using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSStatManager
{
    public class Config
    {
        public string APIKey { get; set; }
        public string Host { get; set; }
        public int Port { get; set; }

        public static Config Load(string file = "config.json")
        {
            string text = File.ReadAllText(file);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(text);
        }

        public void ToFile(string file = "config.json")
        {
            File.WriteAllText(file, Newtonsoft.Json.JsonConvert.SerializeObject(this));
        }
    }
}
