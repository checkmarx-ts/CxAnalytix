using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using CxAnalytics.Configuration;
using log4net;


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

            // TODO: Execute transformation logic.  Pass configuration parameters, etc.
            TransformLogic.Transformer.doTransform(2);

            appLog.Info("End");

            // TODO: Example of using a "record logger" to output a record.  We can use different loggers
            // in log4net to send formatted data out to a file.  Log a JSON payload and it gets written to a file
            // with an ISO 8601 timestamp in front of it.

            for (int x = 0; x < 90; x++)
            {
                recordScanSummaryLog.Info("{ \"foo\" : \"bar\", \"baz\" : \"buz\"}");
                Task.Delay(1000).Wait();
            }

        }
    }
}
