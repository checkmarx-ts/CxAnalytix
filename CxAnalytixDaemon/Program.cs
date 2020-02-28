using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading.Tasks;


[assembly: log4net.Config.XmlConfigurator(ConfigFile = "CxAnalytixDaemon.log4net", Watch = true)]

namespace CxAnalytixDaemon
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var builder = new HostBuilder ().ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<IHostedService, Daemon>();
            });

            await builder.RunConsoleAsync();
        }
    }
}
