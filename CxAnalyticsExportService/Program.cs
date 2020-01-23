using CxAnalytics.Configuration;
using log4net;
using System;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;

namespace CxAnalyticsExportService
{
    class Program
    {
        private static ILog _log = LogManager.GetLogger(typeof (Program));

        static void Main(string[] args)
        {
            try
            {
                Directory.SetCurrentDirectory(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName));
                ServiceBase.Run(new ServiceLifecycleControl());
            }
            catch (Exception ex)
            {
                _log.Error("Unhandled exception caught, service quit.", ex);
            }
        }
    }
}
