using CxAnalytix.Configuration.Impls;
using CxAnalytix.Extensions;
using CxAnalytix.XForm.Common;
using CxRestClient;
using CxRestClient.CXONE;
using log4net;
using OutputBootstrapper;
using SDK.Modules.Transformer.Data;
using System.Collections.Concurrent;
using static SDK.Modules.Transformer.Data.ScanDescriptor;
using CxOneConnection = CxAnalytix.XForm.CxOneTransformer.Config.CxOneConnection;

namespace CxAnalytix.XForm.CxOneTransformer
{
    public class Transformer : BaseTransformer
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Transformer));
        private static readonly String STATE_STORAGE_FILE = "CxAnalytixExportState_CxOne.json";
        private static readonly String MODULE_NAME = "CxOne";

        public Transformer() : base(MODULE_NAME, typeof(Transformer), STATE_STORAGE_FILE)
        {
        }

        public override string DisplayName => "CheckmarxOne";

        public override void DoTransform(CancellationToken token)
        {
            ThreadOpts.CancellationToken = token;

            var conCfg = Configuration.Impls.Config.GetConfig<CxOneConnection>();
            var creds = Configuration.Impls.Config.GetConfig<CxApiTokenCredentials>();


            var restBuilder = new CxOneRestContext.CxOneRestContextBuilder();
            restBuilder.WithApiURL(conCfg.URL)
                .WithIAMUrl(conCfg.IamUrl)
                .WithOpTimeout(conCfg.TimeoutSeconds)
                .WithSSLValidate(conCfg.ValidateCertificates)
                .WithApiToken(creds.Token)
                .WithTenant(creds.Tenant)
                .WithRetryLoop(conCfg.RetryLoop);

            var ctx = restBuilder.Build();


            _log.Debug("Starting CxOne transform");

            var groupsTask = CxGroups.GetGroups(ctx, token);
            var projectsTask = CxProjects.GetProjects(ctx, token);
            var lastScansTask = CxProjects.GetProjectLatestCompletedScans(ctx, token);

            // TODO: applications


            _log.Debug("Resolving projects.");
            var projectDescriptors = new ConcurrentBag<ProjectDescriptor>();

            Parallel.ForEach(projectsTask.Result, ThreadOpts, (p) =>
            {

                String groupsString = String.Join(",", p.Groups.ConvertAll((groupId) => groupsTask.Result[groupId].Path));

                // Projects don't need to have a team assignment, unlike in SAST
                if (!((p.Groups.Count == 0) ? Filter.Matches(p.ProjectName)
                    : p.Groups.Any((t) => Filter.Matches(groupsTask.Result[t].Path, p.ProjectName))))
                {
                    if (_log.IsDebugEnabled)
                        _log.Debug($"FILTERED: Project: [{p.ProjectName}] with assigned groups " +
                            $"[{groupsString}]");
                    return;
                }
                else
                {
                    projectDescriptors.Add(new ProjectDescriptor()
                    {
                        ProjectId = p.ProjectId,
                        ProjectName = p.ProjectName,
                        TeamName = groupsString
                        
                    });
                }
            });

            State.ConfirmProjects(projectDescriptors);

            _log.Info($"{State.ProjectCount} projects are targets to check for new scans. Since last crawl: {State.DeletedProjects}"
                + $" projects removed, {State.NewProjects} new projects.");

            _log.Debug("Resolving scans.");

            Parallel.ForEach(State.Projects, ThreadOpts, (projDescriptor) =>
            {
                var latestScanDateForProject = lastScansTask.Result[projDescriptor.ProjectId].Completed;

                // This skips some API I/O since we know the last scan date of some projects.
                if (projDescriptor.LastScanCheckDate.CompareTo(latestScanDateForProject) < 0)
                {
                    var scanCollection = CxScans.GetCompletedScans(ctx, ThreadOpts.CancellationToken, projDescriptor.ProjectId);

                    foreach (var s in scanCollection.Result.Scans)
                    {
                        // Add to crawl state.
                        if (_log.IsTraceEnabled())
                            _log.Trace($"CxOne scan record: {s}");

                        State.AddScan(Convert.ToString(s.ProjectId), s.SourceOrigin, ScanProductType.CXONE, s.ScanId, s.Updated, s.EnginesAsString);
                    }
                }
                else
                    _log.Info($"Project {projDescriptor.ProjectId}:{projDescriptor.TeamName}:{projDescriptor.ProjectName} has no new scans to process.");

            });

            if (State.Projects == null)
            {
                _log.Error("Scans to crawl do not appear to be resolved, unable to crawl scan data.");
                return;
            }

            _log.Info($"Crawling {State.ScanCount} scans.");

            Parallel.ForEach<ProjectDescriptor>(State.Projects, ThreadOpts,
            (project) =>
            {
                if (State.GetScanCountForProject(project.ProjectId) <= 0)
                    return;

                // TODO: Application data
                // TODO: Scan statistics?
                // TODO: May need to know which type of scan result APIs to retrieve for each scan since a scan can have SAST and SCA scans included in it.
                


                //var riskStateTask = Task.Run(() => CxRiskState.GetRiskStates(ctx, ThreadOpts.CancellationToken, project.ProjectId),
                //    ThreadOpts.CancellationToken);

                using (var pinfoTrx = Output.StartTransaction())
                {
                    OutputProjectInfoRecords(pinfoTrx, project);

                    if (!ThreadOpts.CancellationToken.IsCancellationRequested)
                        pinfoTrx.Commit();
                }



                foreach (var scan in State.GetScansForProject(project.ProjectId))
                {
                    if (ThreadOpts.CancellationToken.IsCancellationRequested)
                        break;


                    using (var scanTrx = Output.StartTransaction())
                    {
                        _log.Info($"Processing {scan.ScanProduct} scan {scan.ScanId}:{scan.Project.ProjectId}:{scan.Project.ProjectName}[{scan.FinishedStamp}]");

                        var rpt = CxScanResults.GetScanResults(ctx, ThreadOpts.CancellationToken, scan.ScanId).Result;

                        //var riskReport = CxDetailedReport.GetDetailedReport(ctx, ThreadOpts.CancellationToken, ScanHeaderIndex[scan.ScanId].RiskReportId);

                        //foreach (var policy in riskReport.Policies)
                        //    if (policy.IsViolating)
                        //        foreach (var rule in policy.Rules)
                        //            if (rule.IsViolated)
                        //                scan.IncrementPolicyViolation(policy.PolicyName, rule.Name);

                        //OutputScanSummary(scanTrx, scan, riskReport);
                        //OutputScanDetails(scanTrx, scan, riskReport, riskStateTask.Result);
                        //OutputPolicyViolations(scanTrx, scan, riskReport, riskStateTask.Result);


                        if (!ThreadOpts.CancellationToken.IsCancellationRequested && scanTrx.Commit())
                            State.ScanCompleted(scan);
                    }
                }


            });

        }
    }
}