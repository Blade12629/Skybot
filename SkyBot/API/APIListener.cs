using Grapevine.Server;
using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.API
{
    public class APIListener : IDisposable
    {
        public static APIListener Listener
        {
            get
            {
                if (_listener == null)
                    _listener = new APIListener();

                return _listener;
            }
        }
        public RestServer Server => _server;

        public bool IsDisposed { get; private set; }


        private static APIListener _listener;
        private RestServer _server;

        public void Start(int port, string host)
        {
            _server = new RestServer(CreateSettings(port, host));

            ////Supress any exceptions
            _server.EnableThrowingExceptions = false;
            _server.Router.SendExceptionMessages = false;

            _server.LogToConsole(Grapevine.Interfaces.Shared.LogLevel.Info);
            _server.Router.BeforeRouting += BeforeRouting;
            _server.Start();
        }

        private void BeforeRouting(Grapevine.Interfaces.Server.IHttpContext context)
        {
            //Allow any cross origin cors requests, without this, websites might not be able to get any data from the API
            context.Response.AddHeader("Access-Control-Allow-Origin", "*");
            context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With");
        }

        ~APIListener()
        {
            Dispose(false);
        }

        private static IServerSettings CreateSettings(int port, string host)
        {
            ServerSettings settings = new ServerSettings()
            {
#pragma warning disable CA1305 // Specify IFormatProvider
                Port = port.ToString(),
#pragma warning restore CA1305 // Specify IFormatProvider
                Host = host
            };

            return settings;
        }

        public void Stop()
        {
            _server.Stop();
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;

            IsDisposed = true;

            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                _server?.Dispose();
            }
        }
    }
}
