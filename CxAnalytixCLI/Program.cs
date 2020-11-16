using System.IO;
using System.Reflection;
using log4net;
using CxAnalytix.TransformLogic;
using System.Threading;
using CxRestClient;
using CxAnalytix.Configuration;
using System;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.AuditTrails.Crawler;

[assembly: CxRestClient.IO.NetworkTraceLog()]
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
            builder.WithSASTServiceURL(Config.Connection.URL)
            .WithOpTimeout(Config.Connection.TimeoutSeconds)
            .WithSSLValidate(Config.Connection.ValidateCertificates)
            .WithUsername(Config.Credentials.Username)
            .WithPassword(Config.Credentials.Password).
            WithMNOServiceURL (Config.Connection.MNOUrl);

            using (CancellationTokenSource t = new CancellationTokenSource())
            {
                var outFactory = MakeFactory();

                CxRestContext ctx = builder.Build();
                Transformer.DoTransform(Config.Service.ConcurrentThreads, 
                    Config.Service.StateDataStoragePath, Config.Service.InstanceIdentifier,
                    ctx, 
                    outFactory,
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


                AuditTrailCrawler.CrawlAuditTrails(outFactory, t.Token);
            }


            appLog.Info("End");
        }
    }
}
