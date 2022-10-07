using CxAnalytix.Configuration.Impls;
using CxAnalytix.Extensions;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.XForm.Common;
using CxRestClient;
using CxRestClient.CXONE;
using CxRestClient.Utility;
using log4net;
using OutputBootstrapper;
using SDK.Modules.Transformer.Data;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Linq;
using static SDK.Modules.Transformer.Data.ScanDescriptor;
using CxOneConnection = CxAnalytix.XForm.CxOneTransformer.Config.CxOneConnection;
using CxOneProjectScanEngineCount = System.Collections.Concurrent.ConcurrentDictionary<string, System.Collections.Generic.Dictionary<string, System.Tuple<int, System.DateTime>>>;


namespace CxAnalytix.XForm.CxOneTransformer
{
    public class Transformer : BaseTransformer
    {
        
        private static readonly ILog _log = LogManager.GetLogger(typeof(Transformer));
        private static readonly String STATE_STORAGE_FILE = "CxAnalytixExportState_CxOne.json";
        private static readonly String MODULE_NAME = "CxOne";

        public Transformer() : base(MODULE_NAME, typeof(Transformer), STATE_STORAGE_FILE)
        {
            ConnectionConfig = Configuration.Impls.Config.GetConfig<CxOneConnection>();
            
            var creds = Configuration.Impls.Config.GetConfig<CxApiTokenCredentials>();

            var restBuilder = new CxOneRestContext.CxOneRestContextBuilder();
            restBuilder.WithApiURL(ConnectionConfig.URL)
                .WithIAMUrl(ConnectionConfig.IamUrl)
                .WithOpTimeout(ConnectionConfig.TimeoutSeconds)
                .WithSSLValidate(ConnectionConfig.ValidateCertificates)
                .WithApiToken(creds.Token)
                .WithTenant(creds.Tenant)
                .WithRetryLoop(ConnectionConfig.RetryLoop);

            Context = restBuilder.Build();
        }

        private CxOneConnection ConnectionConfig { get; set; }

        public override string DisplayName => "CheckmarxOne";

        private CxOneRestContext Context { get; set; }


        private Task<CxProjects.ProjectIndex>? ProjectsFetchTask { get; set; }

        private Task<CxApplications.ApplicationIndex>? ApplicationsFetchTask { get; set; }



        private CxOneProjectScanEngineCount ScanEngineStats { get; set; } = new();


        private static String NormalizeEngineName(String name) => $"{ScanProductType.CXONE}_{name}";


        private void TrackScanEngineStats(String projectId, CxScans.Scan scan)
        {

            lock (ScanEngineStats)
            {
                if (!ScanEngineStats.ContainsKey(projectId))
                {

                    ScanEngineStats.TryAdd(projectId, new Dictionary<string, Tuple<int, DateTime>>(
                        scan.EnginesForScan.Aggregate(new List<KeyValuePair<String, Tuple<int, DateTime>>>(), (bucket, value) =>
                        {
                            bucket.Add(new KeyValuePair<String, Tuple<int, DateTime>>(NormalizeEngineName(value),
                                new Tuple<int, DateTime>(1, scan.Updated)));
                            return bucket;
                        })));
                }
                else
                    foreach (var engine in scan.EnginesForScan)
                    {
                        var normalizedName = NormalizeEngineName(engine);

                        if (!ScanEngineStats[projectId].ContainsKey(normalizedName) )
                            ScanEngineStats[projectId].Add(normalizedName, new Tuple<int, DateTime>(1, scan.Updated));
                        else
                            ScanEngineStats[projectId][normalizedName] = new Tuple<int, DateTime>(ScanEngineStats[projectId][normalizedName].Item1 + 1,
                                (ScanEngineStats[projectId][normalizedName].Item2.CompareTo(scan.Updated) < 0) ? (scan.Updated) :
                                (ScanEngineStats[projectId][normalizedName].Item2));
                    }
            }
        }


        public override void DoTransform(CancellationToken token)
        {
            ThreadOpts.CancellationToken = token;

            _log.Debug("Starting CxOne transform");

            ProjectsFetchTask = CxProjects.GetProjects(Context, token);
            ApplicationsFetchTask = CxApplications.GetApplications(Context, token);

            var groupsTask = CxGroups.GetGroups(Context, token);
            var lastScansTask = CxProjects.GetProjectLatestCompletedScans(Context, token);

            _log.Debug("Resolving projects.");
            var projectDescriptors = new ConcurrentBag<ProjectDescriptor>();

            Parallel.ForEach(ProjectsFetchTask.Result, ThreadOpts, (p) =>
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

            var loadedScans = new CxScans.ScanIndex();


            Parallel.ForEach(State.Projects, ThreadOpts, (projDescriptor) =>
            {
                var latestScanDateForProject = lastScansTask.Result[projDescriptor.ProjectId].Completed;

                // This skips some API I/O since we know the last scan date of some projects.
                if (projDescriptor.LastScanCheckDate.CompareTo(latestScanDateForProject) < 0)
                {
                    var scanCollection = CxScans.GetCompletedScans(Context, ThreadOpts.CancellationToken, projDescriptor.ProjectId);

                    foreach (var s in scanCollection.Result.Scans)
                    {
                        // Add to crawl state.
                        if (_log.IsTraceEnabled())
                            _log.Trace($"CxOne scan record: {s}");

                        State.AddScan(Convert.ToString(s.ProjectId), s.ScanType, ScanProductType.CXONE, s.ScanId, s.Updated, s.EnginesAsString);
                        TrackScanEngineStats(projDescriptor.ProjectId, s);
                    }

                    loadedScans.SyncCombine(scanCollection.Result.Scans);
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



            var scanMetadata = CxSastScanMetadata.GetScanMetadata(Context, token, ThreadOpts, State.ScopeScanIds);

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
                        var projid = (scan.Project == null) ? "Unknown ProjectId" : scan.Project.ProjectId;
                        var projname = (scan.Project == null) ? "Unknown Project Name" : scan.Project.ProjectName;

                        _log.Info($"Processing {scan.ScanProduct} scan {scan.ScanId}:{projid}:{projname}[{scan.FinishedStamp}]");

                        var rpt = CxScanResults.GetScanResults(Context, ThreadOpts.CancellationToken, scan.ScanId).Result;

                        if (rpt.SastResults != null && rpt.SastResults.Count > 0)
                            OutputSastScanResults(scanTrx, project, scan, loadedScans, rpt.SastResults, scanMetadata.Result[scan.ScanId]);

                        if (rpt.ScaResults != null && rpt.ScaResults.Count > 0)
                            OutputScaScanResults(scanTrx, project, scan, rpt.ScaResults);


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

        private void AddPairsAsTags(IDictionary<String, String> from, IDictionary<String, Object> to)
        {
            foreach (var key in from.Keys)
                to.Add($"TAG_{key}", from[key]);
        }

        protected override void AddAdditionalProjectInfo(IDictionary<string, object> here, string projectId)
        {
            base.AddAdditionalProjectInfo(here, projectId);

            if (ProjectsFetchTask != null)
            {
                here.Add("CriticalityLevel", ProjectsFetchTask.Result[projectId].Criticality);
                here.Add("RepoUrl", ProjectsFetchTask.Result[projectId].RepoUrl);
                here.Add("RepoMainBranch", ProjectsFetchTask.Result[projectId].MainBranch);
                AddPairsAsTags(ProjectsFetchTask.Result[projectId].Tags, here);
            }

            if (ApplicationsFetchTask != null && ApplicationsFetchTask.Result.ContainsKey(projectId))
            {
                here.Add("Applications", String.Join(";", ApplicationsFetchTask.Result[projectId].Select((app) => app.Name)));
                here.Add("ApplicationMaxCriticality", ApplicationsFetchTask.Result[projectId].Max((app) => app.Criticality));
                here.Add("ApplicationMinCriticality", ApplicationsFetchTask.Result[projectId].Min((app) => app.Criticality));
            }
        }

        private void OutputScaScanResults(IOutputTransaction scanTrx, ProjectDescriptor project, ScanDescriptor scan, List<CxScanResults.ScaResult> scaResults)
        {
            //throw new NotImplementedException();
        }

        private void OutputSastScanResults(IOutputTransaction scanTrx, ProjectDescriptor project, ScanDescriptor scan, CxScans.ScanIndex scanHeaders, 
            List<CxScanResults.SastResult> sastResults, CxSastScanMetadata.SastScanMetadata metadata)
        {

            var metrics = CxSastScanMetadata.GetScanMetrics(Context, ThreadOpts.CancellationToken, scan.ScanId);
            var flat_summary = new SortedDictionary<String, object>();
            
            int low = 0, medium = 0, high = 0, info = 0;

            AddScanHeaderElements(scan, flat_summary);
            flat_summary.Add("DeepLink", UrlUtils.MakeUrl(ConnectionConfig.DeepLinkUrl, "results", scan.ScanId, project.ProjectId, "sast"));
            flat_summary.Add("SourceOrigin", scanHeaders[scan.ScanId].SourceOrigin);
            flat_summary.Add("SourceType", scanHeaders[scan.ScanId].SourceType);
            flat_summary.Add("Engines", scanHeaders[scan.ScanId].EnginesAsString);
            flat_summary.Add("Initiator", scanHeaders[scan.ScanId].Initiator);

            AddPairsAsTags(scanHeaders[scan.ScanId].Tags, flat_summary);


            flat_summary.Add("Branch", scanHeaders[scan.ScanId].Branch);
            flat_summary.Add("ScanStart", scanHeaders[scan.ScanId].Created);
            flat_summary.Add("ScanFinished", scanHeaders[scan.ScanId].Updated);

            var scanTime = scanHeaders[scan.ScanId].Updated - scanHeaders[scan.ScanId].Created;
            flat_summary.Add("ScanTime", scanTime.ToString(@"hh\h\:mm\m\:ss\s"));

            flat_summary.Add("Preset", metadata.Preset);
            flat_summary.Add("LinesOfCode", metadata.LOC);
            flat_summary.Add("FileCount", metadata.FileCount);

            flat_summary.Add("Languages", String.Join(";", metrics.Result.ScannedFilesByLanguage.Keys) );
            flat_summary.Add("FailedLinesOfCode", 
                metrics.Result.ParseFailureLOCByLanguage.Count > 0 ? metrics.Result.ParseFailureLOCByLanguage.Values.Aggregate( (x, y) => x + y) : 0);





            // TODO: TOTALS FROM DETAILS
            flat_summary.Add("Information", info);
            flat_summary.Add("Low", low);
            flat_summary.Add("Medium", medium);
            flat_summary.Add("High", high);


            scanTrx.write(SastScanSummaryOut, flat_summary);
        }

        protected override void AddProductsLastScanDateFields(IDictionary<string, object> here, ProjectDescriptor project)
        {
            foreach(var engine in ScanEngineStats[project.ProjectId].Keys)
                here.Add($"{engine}_LastScanDate", ScanEngineStats[project.ProjectId][engine].Item2);

        }

        protected override void AddProductsScanCountFields(IDictionary<string, object> here, ProjectDescriptor project)
        {
            foreach (var engine in ScanEngineStats[project.ProjectId].Keys)
                here.Add($"{engine}_Scans", ScanEngineStats[project.ProjectId][engine].Item1);
        }
    }
}