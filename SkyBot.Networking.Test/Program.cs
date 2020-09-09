using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SkyBot.Networking.Test
{
    class Program
    {
        static Irc.OsuIrcClient _client;

        static void Main(string[] args)
            => MainTask().ConfigureAwait(false).GetAwaiter().GetResult();

        static async Task MainTask()
        {
            try
            {
                string user = "Skyfly";
                string pass = "ssssssssssss";

                IPAddress ip = Dns.GetHostEntry("irc.ppy.sh").AddressList.First(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                _client = new Irc.OsuIrcClient();
                _client.OnWelcomeMessageReceived += (s, e) => Console.WriteLine($"Welcome Message: {e.WelcomeMessage}");
                _client.OnPrivateMessageReceived += (s, e) => Console.WriteLine($"Private Message: Sender: {e.Sender} Server: {e.Server} Message: {e.Message}");
                _client.OnChannelMessageReceived += (s, e) => Console.WriteLine($"Channel Message: Sender: {e.Sender} Server: {e.Server} Destination: {e.Destination} Message: {e.Message}");
                _client.OnPrivateBanchoMessageReceived += (s, e) => Console.WriteLine($"Bancho Message: Sender: {e.Sender} Server: {e.Server} Message: {e.Message}");

                await _client.ConnectAsync(reconnectDelay: TimeSpan.FromMinutes(15.0)).ConfigureAwait(false);
                await _client.LoginAsync(user, pass).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            await Task.Delay(-1);
        }
    }
}
