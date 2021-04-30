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
using CxAnalytix.Exceptions;
using ProjectFilter;
using OutputBootstrapper;

[assembly: CxRestClient.IO.NetworkTraceLog()]
[assembly: CxAnalytix.Extensions.LogTrace()]
[assembly: log4net.Config.XmlConfigurator(ConfigFile= "CxAnalytixCLI.log4net", Watch = true)]


namespace CxAnalytixCLI
{
    class Program
    {
        private static readonly ILog appLog = LogManager.GetLogger(typeof(Program));

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
                try
                {
                    CxRestContext ctx = builder.Build();
                    Transformer.DoTransform(Config.Service.ConcurrentThreads,
                        Config.Service.StateDataStoragePath, Config.Service.InstanceIdentifier,
                        ctx,
                        new FilterImpl (Config.GetConfig<CxFilter>("ProjectFilterRegex").TeamRegex,
                        Config.GetConfig<CxFilter>("ProjectFilterRegex").ProjectRegex),
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

					if (!t.Token.IsCancellationRequested)
						using (var auditTrx = Output.StartTransaction())
						{
							AuditTrailCrawler.CrawlAuditTrails(t.Token);

							if (!t.Token.IsCancellationRequested)
								auditTrx.Commit();
						}
				}
				catch (ProcessFatalException pfe)
				{
                    appLog.Error("Fatal exception caught, program ending.", pfe);
				}
                catch (Exception ex)
                {
                    appLog.Error("Unhandled exception caught, program ending.", ex);
                }
            }

            appLog.Info("End");
        }
    }
}
