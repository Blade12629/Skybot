using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Networking.Irc
{
    public class OsuIrcClient : IDisposable
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

        public bool IsDisposed { get; private set; }
        public string CurrentUser { get; private set; }
        public bool IsConnected => _irc?.IsConnected ?? false;

        private IrcClient _irc;
        protected string _lastNick;
        private string _lastPass;

        private System.Timers.Timer _reconnectTimer;
        private TimeSpan? _reconnectDelay;
        private Stopwatch _connectedSince;

        public OsuIrcClient(string host = "irc.ppy.sh", int port = 6667)
        {
            _irc = new IrcClient(host, port, "123", "123", false);
            _irc.OnMessageRecieved += OnRawIrcMessageReceived;
            _connectedSince = new Stopwatch();
        }

        ~OsuIrcClient()
        {
            Dispose(false);
        }

        private void ReconnectWatcher()
        {
            try
            {
                if (!IsConnected)
                {
                    ConnectAsync(false).ConfigureAwait(false).GetAwaiter().GetResult();
                    LoginAsync(_lastNick, _lastPass).ConfigureAwait(false).GetAwaiter().GetResult();
                    return;
                }

                if (_reconnectDelay.HasValue && _reconnectDelay.Value.TotalMilliseconds <= _connectedSince.ElapsedMilliseconds)
                {
                    while (!ReconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult())
                        Task.Delay(500).ConfigureAwait(false).GetAwaiter().GetResult();

                    while (!IsConnected)
                        Task.Delay(250).ConfigureAwait(false).GetAwaiter().GetResult();

                    _connectedSince.Restart();

                    LoginAsync(_lastNick, _lastPass).ConfigureAwait(false).GetAwaiter().GetResult();
                }
            }
            catch (Exception)
            {
            }
        }

        /// <param name="reconnectAndRelogin">If false ignore all other parameters. Checks if we should reconnect + login</param>
        /// <param name="reconnectDelay">Time we can stay connected until we initiate a reconnect, leave empty to not use</param>
        /// <param name="checkConnDelay">Check if we are connected every X ms</param>
        public async Task ConnectAsync(bool reconnectAndRelogin = true, TimeSpan? reconnectDelay = null, double checkConnDelay = 500)
        {
            await Task.Run(() => _irc.Connect()).ConfigureAwait(false);
            _irc.StartReadingAsync();

            if (reconnectAndRelogin)
            {
                _reconnectTimer = new System.Timers.Timer(checkConnDelay)
                {
                    AutoReset = true
                };
                _reconnectTimer.Elapsed += (s, e) => ReconnectWatcher();

                _reconnectDelay = reconnectDelay;
                _reconnectTimer.Start();

                if (reconnectDelay.HasValue)
                {
                    if (_connectedSince?.IsRunning ?? false == true)
                        _connectedSince.Restart();
                    else
                        _connectedSince.Start();
                }
            }
        }

        public async Task DisconnectAsync(bool stopTimer = true)
        {
            if (_connectedSince?.IsRunning ?? false == true)
            {
                _connectedSince.Stop();
                _connectedSince.Reset();
            }

            if (stopTimer)
                _reconnectTimer.Stop();

            _irc.StopReading();
            _irc.Disconnect();
        }

        public async Task<bool> ReconnectAsync()
        {
            try
            {
                await DisconnectAsync(false).ConfigureAwait(false);
                Task.Delay(500).Wait();
                await ConnectAsync(false).ConfigureAwait(false);
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public async Task LoginAsync(string nick, string pass)
        {
            _lastNick = nick;
            _lastPass = pass;

            await SendCommandAsync("PASS", pass).ConfigureAwait(false);
            await SendCommandAsync("NICK", nick).ConfigureAwait(false);
        }

        public async Task SendCommandAsync(string command, string parameters)
        {
            await WriteAsync($"{command} {parameters}").ConfigureAwait(false);
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

        protected virtual async Task WriteAsync(string message)
        {
            await _irc.WriteAsync(message).ConfigureAwait(false);
        }

        private void OnRawIrcMessageReceived(object sender, string e)
        {
            List<string> msgSplit = e.Split(' ').ToList();

            switch (msgSplit[0].ToLower(CultureInfo.CurrentCulture))
            {
                case "ping":
                    OnPing();
                    return;
            }

            switch (msgSplit[1].ToLower(CultureInfo.CurrentCulture))
            {
                case "join":
                    OnJoinMessage(msgSplit);
                    return;

                case "quit":
                    OnQuitMessage(msgSplit);
                    return;

                case "mode":
                    OnModeMessage(msgSplit, e);
                    return;

                case "privmsg":
                    OnMessage(msgSplit, e);
                    return;

                case "353": //User List (names)
                    OnUserListReceived(msgSplit);
                    return;

                case "366": //User List End
                    OnUserListEndReceived();
                    return;

                case "part":
                    OnUserPart(msgSplit);
                    return;

                case "001": //Welcome Message
                    OnWelcomeMessage(e.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3).TrimStart(':'));
                    return;

                case "332": //Channel topic
                    OnChannelTopic(e.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3).TrimStart(':'));
                    return;

                case "333": //???

                    return;

                case "375": //Motd begin

                    return;

                case "372": //Motd
                    OnMotd(e.Remove(0, msgSplit[0].Length + msgSplit[1].Length + msgSplit[2].Length + 3).TrimStart(':'));
                    return;

                case "376": //Motd end

                    return;

                case "pong":

                    return;
            }

            Console.WriteLine("Unkown command: " + e);
        }

        private void OnJoinMessage(List<string> msgSplit)
        {
            (string, string) userAndServer = ExtractUserAndServer(msgSplit[0]);
            string parameter = msgSplit[2].TrimStart(':');

            OnUserJoined?.Invoke(this, new IrcJoinEventArgs(userAndServer.Item1, userAndServer.Item2, parameter));
        }

        private void OnQuitMessage(List<string> msgSplit)
        {
            (string, string) userAndServer = ExtractUserAndServer(msgSplit[0]);
            string parameter = msgSplit[2].TrimStart(':');

            string channel = null;

            if (IsChannel(parameter))
                channel = parameter;

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
            //for (int i = 0; i < 6; i++)
            //    msgSplit.RemoveAt(i);

            //msgSplit.RemoveAt(msgSplit.Count - 1);

        }

        private void OnUserListEndReceived()
        {

        }

        private void OnUserPart(List<string> msgSplit)
        {
            (string, string) userAndServer = ExtractUserAndServer(msgSplit[0]);
            string channel = msgSplit[2].TrimStart(':');

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
            SendCommandAsync("PONG", "cho.ppy.sh").ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            if (disposing)
            {
                _irc?.Dispose();
                _reconnectTimer?.Dispose();
            }
        }
    }
}
