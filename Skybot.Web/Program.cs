using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SkyBot;

namespace Skybot.Web
{
    public class Program
    {
        public static void Main(string[] args)
        {
            SkyBotConfig.Read(@"D:\reposSSD\SkyBot\Skybot.Web\bin\Debug\netcoreapp3.1\SkyBotConfig.cfg");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });

    }
}
