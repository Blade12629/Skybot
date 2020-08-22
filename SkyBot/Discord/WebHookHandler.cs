using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace SkyBot.Discord
{
    /// <summary>
    /// Used to send embeds or messages via a webhook
    /// </summary>
    public class WebHookHandler : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public DiscordChannel Channel { get; private set; }

        private DiscordWebhook _webhook;
        private Uri _avatarUrl;
        private string _username;

        /// <summary>
        /// Used to send embeds or messages via a webhook
        /// </summary>
        public WebHookHandler(DiscordChannel channel, string username, string avatarUrl = null)
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

        protected virtual void Dispose(bool disposing)
        {
            if (IsDisposed)
                return;

            if (disposing)
                _webhook?.DeleteAsync().ConfigureAwait(false).GetAwaiter().GetResult();

            IsDisposed = true;
        }
    }
}
