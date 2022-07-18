using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Threading;

namespace CxAnalytixService
{
    class Program
    {

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
            catch (Exception)
            {
            }
        }
    }
}
