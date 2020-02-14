using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using log4net;
using CxAnalytics.TransformLogic;
using System.Threading;
using CxRestClient;
using CxAnalytics.Configuration;
using System;

[assembly: log4net.Config.XmlConfigurator(ConfigFile= "CxAnalytixCLI.log4net", Watch = true)]


namespace CxAnalytixCLI
{
    class Program
    {
        private static readonly ILog recordScanSummaryLog = LogManager.Exists(Assembly.GetExecutingAssembly(), "RECORD_SAST_Scan_Summary");
        private static readonly ILog appLog = LogManager.GetLogger(typeof(Program));

        private static IOutputFactory MakeFactory ()
        {
            IOutputFactory retVal = null;
            try
            {
                Assembly outAssembly = Assembly.Load(Config.Service.OutputAssembly);
                appLog.DebugFormat("outAssembly loaded: {0}", outAssembly.FullName);
                retVal = outAssembly.CreateInstance(Config.Service.OutputClass) as IOutputFactory;
                appLog.Debug("IOutputFactory instance created.");
            }
            catch (Exception ex)
            {
                appLog.Error("Error loading output factory.", ex);
            }

            return retVal;
        }

        static void Main(string[] args)
        {

            appLog.Info("Start");

            appLog.InfoFormat("CWD: {0}", Directory.GetCurrentDirectory () );


            var builder = new CxRestContext.CxRestContextBuilder();
            builder.serviceUrl(Config.Connection.URL)
            .withOpTimeout(Config.Connection.TimeoutSeconds)
            .withSSLValidate(Config.Connection.ValidateCertificates)
            .withUsername(Config.Credentials.Username)
            .withPassword(Config.Credentials.Password);

            using (CancellationTokenSource t = new CancellationTokenSource())
            {
                CxRestContext ctx = builder.build();
                Transformer.doTransform(Config.Service.ConcurrentThreads, 
                    Config.Service.StateDataStoragePath, ctx, 
                    MakeFactory (),
                    new RecordNames()
                    {
                        SASTScanSummary = Config.Service.SASTScanSummaryRecordName,
                        SASTScanDetail = Config.Service.SASTScanDetailRecordName,
                        SCAScanSummary = Config.Service.SCAScanSummaryRecordName,
                        SCAScanDetail = Config.Service.SCAScanDetailRecordName,
                        ProjectInfo = Config.Service.ProjectInfoRecordName,
                        PolicyViolations = Config.Service.PolicyViolationsRecordName
                    },
                    t.Token);
            }


            appLog.Info("End");
        }
    }
}
