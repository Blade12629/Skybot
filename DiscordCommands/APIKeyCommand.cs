using SkyBot;
using SkyBot.Database.Models;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class APIKeyCommand : ICommand
    {
        private static Random _random { get; } = new Random();

        public bool IsDisabled
        {
            get
            {
                return false;
            }
            set
            {

            }
        }

        public string Command => "apikey";

        public AccessLevel AccessLevel => AccessLevel.User;

        public CommandType CommandType => CommandType.Public;

        public string Description => "Generates an api key";

        public string Usage => "!apikey";

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            if (args.Guild.Owner.Id != args.User.Id)
            {
                args.Channel.SendMessageAsync("You can only create an api key while being a server owner!");
                return;
            }

            using DBContext c = new DBContext();

            string key = GenerateAPIKey();
            string keyMd5 = Program.GenerateMd5(key);

            APIUser user = c.APIUser.FirstOrDefault(u => u.DiscordUserId == (long)args.User.Id && 
                                                         u.DiscordGuildId == (long)args.Guild.Id);

            if (user != null)
            {
                args.Channel.SendMessageAsync("You already received your api key, to request a new one/extra key contact: ??????#0284");
                return;
            }

            user = new APIUser((long)args.User.Id, (long)args.Guild.Id, keyMd5);
            c.APIUser.Add(user);
            c.SaveChanges();

            var dm = Program.DiscordHandler.Client.CreateDmAsync(args.User).Result;
            dm.SendMessageAsync("Created your api key, you are responsible for this, if you need extra keys contact: ??????#0284: " + key);
        }

        private string GenerateAPIKey(int length = 15)
        {
            List<char> chars = new List<char>()
            {
                'a', 'b', 'c', 'd', 'e', 'f',
                'g', 'h', 'i', 'j', 'k', 'l',
                'm', 'n', 'o', 'p', 'q', 'r',
                's', 't', 'u', 'v', 'w', 'x',
                'y', 'z',
            };

            for (int i = 0; i < 10; i++)
                chars.Add(i.ToString()[0]);

            string key = "";

            for (int i = 0; i < length; i++)
                key += chars[_random.Next(0, chars.Count)];

            return key;
        }
    }
}
