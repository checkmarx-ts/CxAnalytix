using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using CxAnalytics.TransformLogic;
using System.Threading;
using CxRestClient;
using CxAnalytics.Configuration;

[assembly: log4net.Config.XmlConfigurator(ConfigFile="ConsoleRunner.log4net", Watch = true)]


namespace ConsoleRunner
{
    class Program
    {
        private static readonly ILog recordScanSummaryLog = LogManager.Exists(Assembly.GetExecutingAssembly(), "RECORD_SAST_Scan_Summary");
        private static readonly ILog appLog = LogManager.GetLogger(typeof(Program));

        static void Main(string[] args)
        {

            // TODO: Example of how properties can be used to manipulate the file output locations
            // configured in the log4net XML.
            GlobalContext.Properties["basePath"] = "..\\test_";


            appLog.Info("Start");

            appLog.InfoFormat("CWD: {0}", Directory.GetCurrentDirectory () );


            var builder = new CxRestContext.CxRestContextBuilder();
            builder.serviceUrl(Config.Connection.URL)
            .withOpTimeout(Config.Connection.TimeoutSeconds)
            .withSSLValidate(Config.Connection.ValidateCertificates)
            .withUsername(Config.Credentials.Username)
            .withPassword(Config.Credentials.Password);

            using (CancellationTokenSource t = new CancellationTokenSource())
            using (CxRestContext ctx = builder.build())
            {
                Transformer.doTransform(2, Config.Service.StateDataStoragePath, ctx, null, t.Token);
            }


            appLog.Info("End");

            // TODO: Example of using a "record logger" to output a record.  We can use different loggers
            // in log4net to send formatted data out to a file.  Log a JSON payload and it gets written to a file
            // with an ISO 8601 timestamp in front of it.

        }
    }
}
