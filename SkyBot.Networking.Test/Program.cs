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
                IPAddress ip = Dns.GetHostEntry("irc.ppy.sh").AddressList.First(i => i.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);

                _client = new Irc.OsuIrcClient();
                _client.OnPrivateMessageReceived += (s, e) => Console.WriteLine("Private Message: " + e.Message);
                _client.OnChannelMessageReceived += (s, e) => Console.WriteLine("Channel Message: " + e.Message);
                _client.OnPrivateBanchoMessageReceived += (s, e) => Console.WriteLine($"Bancho Msg: {e.Message}");

                await _client.ConnectAsync().ConfigureAwait(false);
                await _client.LoginAsync("Skyfly", "somepassword1234").ConfigureAwait(false);

                await Task.Delay(1500);

                await _client.SendCommandAsync("PRIVMSG", "banchobot !help").ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            await Task.Delay(-1);
        }
    }
}
