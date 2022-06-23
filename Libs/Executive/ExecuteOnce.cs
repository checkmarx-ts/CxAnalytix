using System.IO;
using System.Reflection;
using log4net;
using CxAnalytix.TransformLogic;
using System.Threading;
using CxRestClient;
using CxAnalytix.Configuration.Impls;
using System;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.AuditTrails.Crawler;
using CxAnalytix.Exceptions;
using ProjectFilter;
using OutputBootstrapper;
using CxRestClient.Utility;
using CxAnalytix.Configuration.Contracts;




[assembly: CxRestClient.IO.NetworkTraceLog()]
[assembly: CxAnalytix.Extensions.LogTrace()]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "cxanalytix.log4net", Watch = true)]


namespace CxAnalytix.Executive
{
    public class ExecuteOnce
    {

        private static ICxConnection Connection { get; set; }
        private static ICxCredentials Credentials { get; set; }
        private static ICxAnalytixService Service { get; set; }
        private static ICxFilter Filter { get; set; }


        static ExecuteOnce()
        {
            Service = Config.GetConfig<ICxAnalytixService>();
            Connection = Config.GetConfig<ICxConnection>();
            Credentials = Config.GetConfig<ICxCredentials>();
            Filter = Config.GetConfig<ICxFilter>();
        }


        private static readonly ILog appLog = LogManager.GetLogger(typeof(ExecuteOnce));

        public static void Execute()
        {
            appLog.Info("Start");

            appLog.InfoFormat("CWD: {0}", Directory.GetCurrentDirectory());


            var builder = new CxSASTRestContext.CxSASTRestContextBuilder();
            builder.WithServiceURL(Connection.URL)
            .WithOpTimeout(Connection.TimeoutSeconds)
            .WithSSLValidate(Connection.ValidateCertificates)
            .WithUsername(Credentials.Username)
            .WithPassword(Credentials.Password)
            .WithMNOServiceURL(Connection.MNOUrl)
            .WithRetryLoop(Connection.RetryLoop);

            using (CancellationTokenSource t = new CancellationTokenSource())
            {
                try
                {
                    CxSASTRestContext ctx = builder.Build();
                    Transformer.DoTransform(Service.ConcurrentThreads,
                        Service.StateDataStoragePath, Service.InstanceIdentifier,
                        ctx,
                        new FilterImpl(Filter.TeamRegex, Filter.ProjectRegex),
                        new RecordNames()
                        {
                            SASTScanSummary = Service.SASTScanSummaryRecordName,
                            SASTScanDetail = Service.SASTScanDetailRecordName,
                            SCAScanSummary = Service.SCAScanSummaryRecordName,
                            SCAScanDetail = Service.SCAScanDetailRecordName,
                            ProjectInfo = Service.ProjectInfoRecordName,
                            PolicyViolations = Service.PolicyViolationsRecordName
                        },
                        t.Token,
                        !String.IsNullOrEmpty(Connection.MNOUrl),
                        LicenseChecks.OsaIsLicensed(ctx, t.Token));

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