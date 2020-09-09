using NetCoreServer;
using SkyBot.Networking.Irc.Entities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Networking.Irc
{
    public class OsuIrcClient
    {
        public event EventHandler<IrcJoinEventArgs> OnUserJoined;
        public event EventHandler<IrcQuitEventArgs> OnUserQuit;
        public event EventHandler<IrcModeEventArgs> OnUserMode;
        public event EventHandler<IrcPartEventArgs> OnUserParted;
        public event EventHandler<IrcPrivateMessageEventArgs> OnPrivateMessageReceived;
        public event EventHandler<IrcPrivateMessageEventArgs> OnPrivateBanchoMessageReceived;
        public event EventHandler<IrcChannelMessageEventArgs> OnChannelMessageReceived;
        public event EventHandler<IrcWelcomeMessageEventArgs> OnWelcomeMessageReceived;
        public event EventHandler<IrcChannelTopicEventArgs> OnChannelTopicReceived;
        public event EventHandler<IrcMotdEventArgs> OnMotdReceived;

        public string CurrentUser { get; private set; }
        public IReadOnlyDictionary<string, IrcMember> Members => _members;
        public IReadOnlyDictionary<string, IrcChannel> Channels => _channels;

        private IrcClient _irc;

        private ConcurrentDictionary<string, IrcMember> _members;
        private ConcurrentDictionary<string, IrcChannel> _channels;

        public OsuIrcClient(string host = "irc.ppy.sh", int port = 6667)
        {
            _irc = new IrcClient(host, port, "123", "123", false);
            _irc.OnMessageRecieved += OnRawIrcMessageReceived;
            _members = new ConcurrentDictionary<string, IrcMember>();
            _channels = new ConcurrentDictionary<string, IrcChannel>();
        }

        public async Task ConnectAsync()
        {
            await Task.Run(() => _irc.Connect()).ConfigureAwait(false);
            _irc.StartReadingAsync();
        }

        public async Task DisconnectAsync()
        {
            _irc.StopReading();
            await Task.Run(() => _irc.Disconnect()).ConfigureAwait(false);
        }

        public async Task LoginAsync(string nick, string pass)
        {
            await SendCommandAsync("PASS", pass).ConfigureAwait(false);
            await SendCommandAsync("NICK", nick).ConfigureAwait(false);
        }

        public async Task SendCommandAsync(string command, string parameters)
        {
            await _irc.WriteAsync($"{command} {parameters}").ConfigureAwait(false);
        }

        public async Task JoinChannelAsync(string channel)
        {
            if (channel[0] != '#')
                channel = "#" + channel;

            await SendCommandAsync("JOIN", channel);
        }

        public async Task PartChannelAsync(string channel)
        {
            if (channel[0] != '#')
                channel = "#" + channel;

            await SendCommandAsync("PART", channel);
        }

        public async Task SendMessageAsync(string destination, string message)
        {
            await SendCommandAsync("PRIVMSG", $"{destination} {message}").ConfigureAwait(false);
        }

        private void OnRawIrcMessageReceived(object sender, string e)
        {
            List<string> msgSplit = e.Split(' ').ToList();

            if (msgSplit[0].Equals("ping", StringComparison.InvariantCulture))
            {
                OnPing();
                return;
            }

            switch (msgSplit[1].ToLower(CultureInfo.CurrentCulture))
            {
                case "join":
                    OnJoinMessage(msgSplit);
                    break;

                case "quit":
                    OnQuitMessage(msgSplit);
                    break;

                case "mode":
                    OnModeMessage(msgSplit, e);
                    break;

                case "privmsg":
                    OnMessage(msgSplit, e);
                    break;

                case "353": //User List (names)
                    OnUserListReceived(msgSplit);
                    break;

                case "366": //User List End
                    OnUserListEndReceived();
                    break;

                case "part":
                    OnUserPart(msgSplit);
                    break;

                case "001": //Welcome Message
                    OnWelcomeMessage(e.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3).TrimStart(':'));
                    break;

                case "332": //Channel topic
                    OnChannelTopic(e.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3).TrimStart(':'));
                    break;

                case "333": //???

                    break;

                case "375": //Motd begin
                    
                    break;

                case "372": //Motd
                    OnMotd(e.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3).TrimStart(':'));
                    break;

                case "376": //Motd end

                    break;

                default:
                    Console.WriteLine("Unkown command: " + e);
                    break;
            }
        }

        private void OnJoinMessage(List<string> msgSplit)
        {
            (string, string) userAndServer = ExtractUserAndServer(msgSplit[0]);
            string parameter = msgSplit[2].TrimStart(':');

            if (IsChannel(parameter))
            {
                _channels.TryAdd(parameter, new IrcChannel(this, parameter));
                _members.TryGetValue(userAndServer.Item1, out IrcMember member);

                _channels[parameter].OnMemberJoin(member);
            }

            OnUserJoined?.Invoke(this, new IrcJoinEventArgs(userAndServer.Item1, userAndServer.Item2, parameter));
        }

        private void OnQuitMessage(List<string> msgSplit)
        {
            (string, string) userAndServer = ExtractUserAndServer(msgSplit[0]);
            string parameter = msgSplit[2].TrimStart(':');

            string channel = null;

            if (IsChannel(parameter))
                channel = parameter;

            _members.TryRemove(userAndServer.Item1, out IrcMember member);

            if (member != null)
            {
                List<IrcChannel> channels = _channels.Select(kvp => kvp.Value).ToList();

                foreach (IrcChannel ch in channels)
                    ch.OnMemberPart(member);
            }

            OnUserQuit?.Invoke(this, new IrcQuitEventArgs(userAndServer.Item1, userAndServer.Item2, channel));
        }

        private void OnModeMessage(List<string> msgSplit, string line)
        {
            (string, string) userAndServer = ExtractUserAndServer(msgSplit[0]);
            string parameters = line.Remove(0, msgSplit[0].Length + msgSplit[1].Length + 2);

            OnUserMode?.Invoke(this, new IrcModeEventArgs(userAndServer.Item1, userAndServer.Item2, parameters));
        }

        private void OnMessage(List<string> msgSplit, string line)
        {
            (string, string) userAndServer = ExtractUserAndServer(msgSplit[0]);

            bool isChannel = IsChannel(msgSplit[2]);
            string msg = line.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3).TrimStart(':');

            if (isChannel)
                OnChannelMessage(userAndServer.Item1, userAndServer.Item2, msgSplit[2], msg);
            else
                OnPrivateMessage(userAndServer.Item1, userAndServer.Item2, msgSplit[2], msg);
        }

        private void OnPrivateMessage(string sender, string server, string destUser, string message)
        {
            if (sender.Equals("banchobot", StringComparison.CurrentCultureIgnoreCase))
                OnPrivateBanchoMessage(new IrcPrivateMessageEventArgs(sender, server, destUser, message));
            else
                OnPrivateMessageReceived?.Invoke(this, new IrcPrivateMessageEventArgs(sender, server, destUser, message));
        }

        private void OnChannelMessage(string sender, string server, string destChannel, string message)
        {
            OnChannelMessageReceived?.Invoke(this, new IrcChannelMessageEventArgs(sender, server, destChannel, message));
        }

        private void OnUserListReceived(List<string> msgSplit)
        {
            for (int i = 0; i < 6; i++)
                msgSplit.RemoveAt(i);

            msgSplit.RemoveAt(msgSplit.Count - 1);

            for (int i = 0; i < msgSplit.Count; i++)
            {
                if (_members.ContainsKey(msgSplit[i]))
                    continue;

                _members.TryAdd(msgSplit[i], new IrcMember(this, msgSplit[i]));
            }
        }

        private void OnUserListEndReceived()
        {

        }

        private void OnUserPart(List<string> msgSplit)
        {
            (string, string) userAndServer = ExtractUserAndServer(msgSplit[0]);
            string channel = msgSplit[2].TrimStart(':');

            _channels.TryAdd(channel, new IrcChannel(this, channel));
            _members.TryGetValue(userAndServer.Item1, out IrcMember member);

            _channels[channel].OnMemberPart(member);

            OnUserParted?.Invoke(this, new IrcPartEventArgs(userAndServer.Item1, userAndServer.Item2, channel));
        }

        private void OnPrivateBanchoMessage(IrcPrivateMessageEventArgs message)
        {
            OnPrivateBanchoMessageReceived?.Invoke(this, message);
        }

        private bool IsChannel(string channel)
        {
            return channel[0] == '#';
        }

        private (string, string) ExtractUserAndServer(string msg)
        {
            msg = msg.TrimStart(':').TrimEnd(' ');

            string[] split = msg.Split('!');

            return (split[0], split[1]);
        }

        private void ExtractUserAndServer(string msg, out string sender, out string server)
        {
            (string, string) result = ExtractUserAndServer(msg);
            sender = result.Item1;
            server = result.Item2;
        }

        private void OnWelcomeMessage(string msg)
        {
            OnWelcomeMessageReceived?.Invoke(this, new IrcWelcomeMessageEventArgs(msg));
        }

        private void OnChannelTopic(string msg)
        {
            OnChannelTopicReceived?.Invoke(this, new IrcChannelTopicEventArgs(msg));
        }

        private void OnMotd(string msg)
        {
            OnMotdReceived?.Invoke(this, new IrcMotdEventArgs(msg));
        }

        private void OnPing()
        {
            SendCommandAsync("PING", "cho.ppy.sh").ConfigureAwait(false);
        }
    }
}
