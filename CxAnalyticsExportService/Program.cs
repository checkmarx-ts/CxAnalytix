using CxAnalytics.Configuration;
using System;
using System.Configuration;
using System.ServiceProcess;

namespace CxAnalyticsExportService
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine(Config.Credentials.Username);
            Console.WriteLine(Config.Credentials.Password);
            Console.WriteLine(Config.Credentials.Token);
            Console.WriteLine(Config.Connection.URL);
            Console.WriteLine(Config.Connection.TimeoutSeconds);
            Console.WriteLine(Config.Connection.ValidateCertificates);
            Console.WriteLine(Config.Service.ConcurrentThreads);
            Console.WriteLine(Config.Service.StateDataFile);


            //ServiceBase.Run(new ServiceLifecycleControl());
        }
    }
}
