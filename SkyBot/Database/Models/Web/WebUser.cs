using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Database.Models.Web
{
    public class WebUser
    {
        public long Id { get; set; }
        public string Username { get; set; }
        public string PasswordHashed { get; set; }
        public long DiscordUserId { get; set; }

        public WebUser(string username, string passHashed)
        {
            Username = username;
            PasswordHashed = passHashed;
        }

        public WebUser(long discordUserId, string username, string passHashed) : this(username, passHashed)
        {
            DiscordUserId = discordUserId;
        }

        public WebUser()
        {

        }
    }
}
