using CxAnalytix.Configuration;
using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace CxAnalytixService
{
    class Program
    {
        private static ILog _log = LogManager.GetLogger(typeof (Program));

        internal static CancellationTokenSource _progToken = new CancellationTokenSource();

        static void Main(string[] args)
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                var srv = new ServiceLifecycleControl();
                _progToken.Token.Register(() => srv.Stop());
                ServiceBase.Run(srv);
            }
            catch (Exception ex)
            {
                _log.Error("Unhandled exception caught, service quit.", ex);
            }
        }
    }
}
