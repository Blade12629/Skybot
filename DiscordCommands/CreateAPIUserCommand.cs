using SkyBot;
using SkyBot.API;
using SkyBot.Database.Models;
using SkyBot.Discord.CommandSystem;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace DiscordCommands
{
    public class CreateAPIUserCommand : ICommand
    {
        public bool IsDisabled { get; set; }

        public string Command => ResourcesCommands.CreateAPIUserCommand;

        public AccessLevel AccessLevel => AccessLevel.Host;

        public CommandType CommandType => CommandType.Public;

        public string Description => ResourcesCommands.CreateAPIUserCommandDescription;

        public string Usage => ResourcesCommands.CreateAPIUserCommandUsage;

        public int MinParameters => 0;

        private static char[] _possibleKeyChars = new char[]
        {
            'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o','p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',
            '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
        };

        public void Invoke(CommandHandler handler, CommandEventArg args)
        {
            using DBContext c = new DBContext();
            APIUser user = c.APIUser.FirstOrDefault(u => u.DiscordUserId == (long)args.User.Id);

            if (user != null)
            {
                args.Channel.SendMessageAsync(ResourcesCommands.CreateAPIUserCommandAlreadyCreated);
                return;
            }

            string key = GenerateKey();
            string hashedKey = APIAuth.HashKey(key);

            user = new APIUser((long)args.User.Id, hashedKey);

            c.APIUser.Add(user);
            c.SaveChanges();

            args.Member.CreateDmChannelAsync().ConfigureAwait(false).GetAwaiter().GetResult()
                       .SendMessageAsync(ResourcesCommands.CreateAPIUserCommandCreated + $"\n{ResourcesCommands.CreateAPIUserCommandKey} {key}\n{ResourcesCommands.CreateAPIUserCommandNeverShare}");
        }

        private static string GenerateKey()
        {
            StringBuilder keyBuilder = new StringBuilder();

            for (int i = 0; i < 64; i++)
            {
                char c = _possibleKeyChars[Program.Random.Next(0, _possibleKeyChars.Length)];

                if (char.IsLetter(c) && Program.Random.Next(0, 2) != 0)
                    c = char.ToUpper(c, CultureInfo.CurrentCulture);

                keyBuilder.Append(c);
            }

            return keyBuilder.ToString();
        }
    }
}
