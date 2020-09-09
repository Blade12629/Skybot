using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Networking.Irc.Entities
{
    public class IrcMember
    {
        public string Name { get; }

        private OsuIrcClient _client;

        public IrcMember(OsuIrcClient client, string name)
        {
            _client = client;
            Name = name;
        }

        public async Task SendPrivateMessageAsync(string message)
        {
            await _client.SendMessageAsync(Name, message).ConfigureAwait(false);
        }
    }
}
