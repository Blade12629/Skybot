using SkyBot.Networking.Irc;
using SkyBot.Ratelimits;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace SkyBot.Osu.IRC
{
    public sealed class OsuIrcClient : SkyBot.Networking.Irc.OsuIrcClient
    {
        public string Host { get; private set; }
        public int Port { get; private set; }
        public string Nick { get; private set; }
        public char CommandPrefix { get; set; }

        public event EventHandler<IrcPrivateMessageEventArgs> OnUserCommand;

        private QueueRateLimiter _qrl;

        public OsuIrcClient(string host = "irc.ppy.sh", int port = 6667, char commandPrefix = '!') : base(host, port)
        {
            CommandPrefix = commandPrefix;
            Host = host;
            Port = port;
            _qrl = new QueueRateLimiter(0, SkyBotConfig.IrcRateLimitMax, TimeSpan.FromMilliseconds(SkyBotConfig.IrcRateLimitResetDelayMS));
            OnPrivateMessageReceived += OnPrivateMessage;
        }

        ~OsuIrcClient()
        {
            Dispose(false);
        }

        protected override async Task WriteAsync(string message)
        {
            await Task.Run(() => _qrl.Increment<bool>(new Action(async () => await base.WriteAsync(message).ConfigureAwait(false)))).ConfigureAwait(false);
        }

        private void OnPrivateMessage(object sender, IrcPrivateMessageEventArgs args)
        {
            if (args.Message[0].Equals(CommandPrefix))
                OnUserCommand?.Invoke(sender, args);
        }

        protected override void Dispose(bool disposing)
        {
            if (!IsDisposed && disposing)
            {
                _qrl?.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
