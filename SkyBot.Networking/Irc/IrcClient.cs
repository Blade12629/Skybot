﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SkyBot.Networking.Irc
{
    /// <summary>
    /// Do NOT use this, use <see cref="OsuIrcClient"/>
    /// </summary>
    public class IrcClient : IDisposable
    {
        public bool IsValid => !string.IsNullOrEmpty(Host) && Port > 0 &&
                                !string.IsNullOrEmpty(User) &&
                                !string.IsNullOrEmpty(Password);
        public bool IsConnected => _client?.Connected ?? false;

        public string Host { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public bool IPv6 { get; set; }

        public event EventHandler<string> OnMessageRecieved;

        private TcpClient _client;
        private Stream _stream;
        private StreamReader _reader;
        private StreamWriter _writer;

        private bool _isDisposed;

        private Task _readerTask;
        private CancellationToken _readerToken;
        private CancellationTokenSource _readerSource;

        public IrcClient(string host, int port, string user, string password, bool ipv6)
        {
            Host = host;
            Port = port;
            User = user;
            Password = password;
            IPv6 = ipv6;
        }

        public void Connect()
        {
                _client = new TcpClient(AddressFamily.InterNetwork);
                IPAddress ip = Dns.GetHostEntry(Host).AddressList.First(i => i.AddressFamily == AddressFamily.InterNetwork);
                _client.Connect(new IPEndPoint(ip, Port));
                _stream = NetworkStream.Synchronized(_client.GetStream());
                _reader = new StreamReader(_stream);
                _writer = new StreamWriter(_stream);
        }

        public void Disconnect()
        {
            _client.Close();
            _client = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_isDisposed)
                return;

            if (disposing)
            {
                StopReading();
                _reader?.Dispose();
                _writer?.Dispose();

                _client?.Close();
                _client?.Dispose();
            }

            _isDisposed = true;
        }

        public async Task<string> ReadAsync()
        {
            try
            {
                if (_reader == null)
                    return null;

                string line = await _reader.ReadLineAsync().ConfigureAwait(true);

                if (line == null)
                    return null;

                return line;
            }
            catch (ObjectDisposedException)
            {
                return null;
            }
        }

        public void StartReadingAsync()
        {
            _readerSource = new CancellationTokenSource();
            _readerToken = _readerSource.Token;

            _readerTask = new Task(() =>
            {
                while (true)
                {
                    string line = ReadAsync().Result;

                    if (string.IsNullOrEmpty(line))
                        continue;

                    Task.Run(() => OnMessageRecieved?.Invoke(this, line)).ConfigureAwait(false);
                }
            }, _readerToken);
            _readerTask.Start();
        }

        public void StopReading()
        {
            if (_readerTask != null && _readerTask.Status != TaskStatus.Running)
                return;

            _readerSource.Cancel();
        }

        public async Task<bool> WriteAsync(string line)
        {
            try
            {
                if (_writer != null)
                {
                    await _writer.WriteLineAsync(line).ConfigureAwait(true);
                    _writer.Flush();

                    return true;
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex); //temp error logging
            }

            return false;
        }
    }
}