using System.Reflection;
using log4net;
using CxAnalytix.TransformLogic;
using CxRestClient;
using CxAnalytix.Configuration.Impls;
using System;
using CxAnalytix.AuditTrails.Crawler;
using ProjectFilter;
using OutputBootstrapper;
using CxRestClient.Utility;




[assembly: CxRestClient.IO.NetworkTraceLog()]
[assembly: CxAnalytix.Extensions.LogTrace()]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "cxanalytix.log4net", Watch = true)]


namespace CxAnalytix.Executive
{
    public class ExecuteOnce
    {
        protected static CxAnalytixService Service => Config.GetConfig<CxAnalytixService>();

        private static CancellationTokenSource _defaultCancelToken = new CancellationTokenSource();


        //static ExecuteOnce()
        //{

        //    var builder = new CxSASTRestContext.CxSASTRestContextBuilder();
        //    builder.WithServiceURL(Connection.URL)
        //    .WithOpTimeout(Connection.TimeoutSeconds)
        //    .WithSSLValidate(Connection.ValidateCertificates)
        //    .WithUsername(Credentials.Username)
        //    .WithPassword(Credentials.Password)
        //    .WithMNOServiceURL(Connection.MNOUrl)
        //    .WithRetryLoop(Connection.RetryLoop);

        //    _ctx = builder.Build();
        //}

        private static readonly ILog appLog = LogManager.GetLogger(typeof(ExecuteOnce));

        public static void Execute(CancellationTokenSource? t = null)
        {
            if (t == null)
                t = _defaultCancelToken;

            var entry = Assembly.GetEntryAssembly();


            appLog.Info($"Start via [{(entry != null ? entry.GetName() : "UNKNOWN")}]");

            appLog.InfoFormat("CWD: {0}", Directory.GetCurrentDirectory());

            DateTime start = DateTime.Now;
            appLog.Info("Starting data transformation.");


            //Transformer.DoTransform(Service.ConcurrentThreads,
            //    Service.StateDataStoragePath, Service.InstanceIdentifier,
            //    _ctx,
            //    new FilterImpl(Filter.TeamRegex, Filter.ProjectRegex),
            //    new RecordNames()
            //    {
            //        SASTScanSummary = Service.SASTScanSummaryRecordName,
            //        SASTScanDetail = Service.SASTScanDetailRecordName,
            //        SCAScanSummary = Service.SCAScanSummaryRecordName,
            //        SCAScanDetail = Service.SCAScanDetailRecordName,
            //        ProjectInfo = Service.ProjectInfoRecordName,
            //        PolicyViolations = Service.PolicyViolationsRecordName
            //    },
            //    t.Token,
            //    !String.IsNullOrEmpty(Connection.MNOUrl),
            //    LicenseChecks.OsaIsLicensed(_ctx, t.Token));


            appLog.InfoFormat("Vulnerability data transformation finished in {0:0.00} minutes.",
                DateTime.Now.Subtract(start).TotalMinutes);

            start = DateTime.Now;

            if (!t.Token.IsCancellationRequested)
                using (var auditTrx = Output.StartTransaction())
                {
                    AuditTrailCrawler.CrawlAuditTrails(t.Token);

                    if (!t.Token.IsCancellationRequested)
                        auditTrx.Commit();
                }

            appLog.InfoFormat("Audit data transformation finished in {0:0.00} minutes.",
                DateTime.Now.Subtract(start).TotalMinutes);


            appLog.Info("End");
        }
    }
}