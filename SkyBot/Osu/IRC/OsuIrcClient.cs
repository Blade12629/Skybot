using NetIrc2;
using NetIrc2.Events;
using SkyBot.Ratelimits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Osu.IRC
{
    public class OsuIrcClient
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Nick { get; private set; }

        public char CommandPrefix;
        public event EventHandler<ChatMessageEventArgs> OnUserCommand;
        public event EventHandler<ChatMessageEventArgs> OnPrivateMessage;
        public event EventHandler<ChatMessageEventArgs> OnSystemMessage;
        public event EventHandler<ChatMessageEventArgs> OnBanchoMessage;
        public event EventHandler<ChatMessageEventArgs> OnChannelMessage;
        public event EventHandler<string> OnWelcomeMessageReceived;
        public event EventHandler<Exception> OnIrcException;

        private string _pass;
        private IrcClient _client;
        private QueueRateLimiter _qrl;

        public OsuIrcClient(string host = "irc.ppy.sh", int port = 6667, char commandPrefix = '!')
        {
            CommandPrefix = commandPrefix;
            Host = host;
            Port = port;
            _client = new IrcClient();
            _client.GotMessage += OnMessage;
            _client.GotWelcomeMessage += OnWelcomeMessage;
            _client.GotIrcError += OnError;
            _qrl = new QueueRateLimiter(0, SkyBotConfig.IrcRateLimitMax, TimeSpan.FromMilliseconds(SkyBotConfig.IrcRateLimitResetDelayMS));
        }

        private void OnError(object sender, IrcErrorEventArgs e)
        {
            Task.Run(() => OnIrcException?.Invoke(this, new Exception($"{e.Error}: {e.Data.ToIrcString().ToString()}")));
        }

        private void OnWelcomeMessage(object sender, SimpleMessageEventArgs e)
        {
            Task.Run(() => OnWelcomeMessageReceived?.Invoke(this, e.Message.ToString()));
        }

        private void OnMessage(object sender, ChatMessageEventArgs e)
        {
            if (e.Sender == null)
                Task.Run(() => OnSystemMessage?.Invoke(this, e));
            else
            {
                string nick = e.Sender.Nickname.ToString();

                if (nick[0] == '#')
                    Task.Run(() => OnChannelMessage?.Invoke(this, e));
                else if (nick.Equals("BanchoBot"))
                    Task.Run(() => OnBanchoMessage?.Invoke(this, e));
                else
                {
                    if (e.Message.ToString()[0] == CommandPrefix)
                        Task.Run(() => OnUserCommand?.Invoke(this, e));
                    else
                        Task.Run(() => OnPrivateMessage?.Invoke(this, e));
                }

            }
        }

        /// <summary>
        /// Sets the current <see cref="Nick"/> and <see cref="_pass"/>
        /// </summary>
        /// <param name="nick">Nickname</param>
        /// <param name="pass">Password</param>
        public void SetAuthentication(string nick, string pass)
        {
            if (string.IsNullOrEmpty(nick))
                throw new ArgumentNullException("Nickname cannot be null or empty", nameof(nick));
            else if (string.IsNullOrEmpty(pass))
                throw new ArgumentNullException("Password cannot be null or empty", nameof(pass));

            Nick = nick;
            _pass = pass;
        }

        /// <summary>
        /// Sends a login request
        /// </summary>
        /// <param name="nick">Empty: take <see cref="Nick"/></param>
        /// <param name="pass">Empty: take <see cref="_pass"/></param>
        public void Login(string nick = null, string pass = null)
        {
            if (!string.IsNullOrEmpty(nick) && !string.IsNullOrEmpty(pass))
                SetAuthentication(nick, pass);

            SendCommand("PASS", _pass);
            SendCommand("NICK", Nick);
        }

        /// <summary>
        /// Sends a command
        /// </summary>
        /// <param name="command">Command</param>
        /// <param name="parameters">Command Parameters</param>
        public void SendCommand(string command, params string[] parameters)
        {
            _client.IrcCommand(new IrcString(command), parameters?.Select(p => new IrcString(p)).ToArray() ?? null);
        }

        /// <summary>
        /// Sends a message
        /// </summary>
        /// <param name="destination">Destination (Channel/User)</param>
        /// <param name="message">Message</param>
        public void SendMessage(string destination, string message)
        {
            _qrl.Increment<bool>(new Action(() => _client.Message(new IrcString(destination), new IrcString(message))));
        }

        /// <summary>
        /// Connects and automatically logs in
        /// </summary>
        /// <returns></returns>
        public async Task ConnectAndLoginAsync()
        {
            await ConnectAsync();

            while (!_client.IsConnected)
                await Task.Delay(5);

            Login();
        }

        public void Connect()
        {
            _client.Connect(Host, Port);
        }
        
        public async Task ConnectAsync()
        {
            await Task.Run(() => Connect());
        }

        public void Disconnect()
        {
            _client.Close();
        }

        public async Task DisconnectAsync()
        {
            await Task.Run(() => Disconnect());
        }

        public void Reconnect()
        {
            ReconnectAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task ReconnectAsync()
        {
            await DisconnectAsync();
            await ConnectAndLoginAsync();
        }
    }
}
