using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;


[assembly: CxRestClient.IO.NetworkTraceLog()]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "CxAnalytixDaemon.log4net", Watch = true)]

namespace CxAnalytixDaemon
{
    class Program
    {
        internal static CancellationTokenSource _tokenSrc = new CancellationTokenSource();

        static async Task Main(string[] args)
        {
            var builder = new HostBuilder ().ConfigureServices((hostContext, services) =>
            {
                var serviceCol = services.AddSingleton<IHostedService, Daemon>();
            });

            await builder.RunConsoleAsync(_tokenSrc.Token);
        }
    }
}
