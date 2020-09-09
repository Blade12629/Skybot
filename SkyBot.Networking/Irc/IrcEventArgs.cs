using System;
using System.Collections.Generic;
using System.Text;

namespace SkyBot.Networking.Irc
{
    public class IrcWelcomeMessageEventArgs : EventArgs
    {
        public string WelcomeMessage { get; }

        public IrcWelcomeMessageEventArgs(string welcomeMessage)
        {
            WelcomeMessage = welcomeMessage;
        }
    }

    public class IrcChannelTopicEventArgs : EventArgs
    {
        public string Topic { get; }

        public IrcChannelTopicEventArgs(string topic)
        {
            Topic = topic;
        }
    }

    public class IrcMotdEventArgs : EventArgs
    {
        public string Motd { get; }

        public IrcMotdEventArgs(string motd)
        {
            Motd = motd;
        }
    }

    public class IrcPartEventArgs : IrcQuitEventArgs
    {
        public IrcPartEventArgs(string sender, string server, string channel) : base(sender, server, channel)
        {

        }
    }

    public class IrcPrivateMessageEventArgs : EventArgs
    {
        public string Sender { get; }
        public string Server { get; }
        public string Destination { get; }
        public string Message { get; }

        public IrcPrivateMessageEventArgs(string sender, string server, string destination, string message) : base()
        {
            Sender = sender;
            Server = server;
            Destination = destination;
            Message = message;
        }
    }

    public class IrcChannelMessageEventArgs : IrcPrivateMessageEventArgs
    {
        public IrcChannelMessageEventArgs(string sender, string server, string destination, string message) : base(sender, server, destination, message)
        {
        }
    }

    public class IrcModeEventArgs : EventArgs
    {
        public string Sender { get; }
        public string Server { get; }
        public string Parameters { get; }

        public IrcModeEventArgs(string sender, string server, string parameters) : base()
        {
            Sender = sender;
            Server = server;
            Parameters = parameters;
        }
    }

    public class IrcJoinEventArgs : EventArgs
    {
        public string Sender { get; }
        public string Server { get; }
        public string Channel { get; }

        public IrcJoinEventArgs(string sender, string server, string channel) : base()
        {
            Sender = sender;
            Server = server;
            Channel = channel;
        }
    }

    public class IrcQuitEventArgs : IrcJoinEventArgs
    {
        public IrcQuitEventArgs(string sender, string server, string channel) : base(sender, server, channel)
        {

        }
    }
}
