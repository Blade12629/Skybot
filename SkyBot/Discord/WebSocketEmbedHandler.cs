using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Discord
{
    public class WebSocketEmbedHandler : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public DiscordChannel Channel { get; private set; }

        private DiscordWebhook _webhook;
        private Uri _avatarUrl;
        private string _username;

        public WebSocketEmbedHandler(DiscordChannel channel, string username, string avatarUrl = null)
        {
            if (channel == null)
                throw new ArgumentNullException(nameof(channel));
            else if (string.IsNullOrEmpty(username))
                throw new ArgumentNullException(nameof(username));

            if (!string.IsNullOrEmpty(avatarUrl))
                _avatarUrl = new Uri(avatarUrl);

            _username = username;
            _webhook = channel.CreateWebhookAsync(username).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public async Task SendEmbed(string content = null, params DiscordEmbed[] embeds)
        {
            await _webhook.ExecuteAsync(content: content, username: _username, embeds: embeds).ConfigureAwait(false);
        }

        public async Task SendMessage(string message)
        {
            await _webhook.ExecuteAsync(content: message, username: _username).ConfigureAwait(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool dispose)
        {
            if (IsDisposed)
                return;

            _webhook?.DeleteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            IsDisposed = true;
        }
    }
}
