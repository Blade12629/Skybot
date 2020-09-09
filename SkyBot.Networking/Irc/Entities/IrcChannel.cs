using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Networking.Irc.Entities
{
    public class IrcChannel
    {
        public event EventHandler<IrcMember> OnMemberJoined;
        public event EventHandler<IrcMember> OnMemberParted;

        public string Name { get; }
        
        private OsuIrcClient _client;
        private ConcurrentDictionary<string, IrcMember> _members;

        public IrcChannel(OsuIrcClient client, string channel)
        {
            if (channel[0] != '#')
                channel = "#" + channel;

            Name = channel;
            _client = client;
        }

        public async Task SendMessageAsync(string msg)
        {
            await _client.SendMessageAsync(Name, msg).ConfigureAwait(false);
        }

        public IrcMember GetMember(string name)
        {
            if (string.IsNullOrEmpty(name) ||
                !_members.TryGetValue(name, out IrcMember member))
                return null;

            return member;
        }

        public void OnMemberJoin(IrcMember member)
        {
            if (_members.TryAdd(member.Name, member))
                OnMemberJoined?.Invoke(this, member);
        }

        public void OnMemberPart(IrcMember member)
        {
            if (_members.TryRemove(member.Name, out _))
                OnMemberParted?.Invoke(this, member);
        }
    }
}
