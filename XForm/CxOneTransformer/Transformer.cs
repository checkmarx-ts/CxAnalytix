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
using System;
using System.Collections.Concurrent;
using System.Text;
using System.Web;
using static CxRestClient.CXONE.CxConfiguration;
using static CxRestClient.CXONE.CxSastPredicates;
using static CxRestClient.SCA.CxRiskState;
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

        private static readonly String NE_VALUE = "NOT_EXPLOITABLE";

        public Transformer() : base(MODULE_NAME, typeof(Transformer), STATE_STORAGE_FILE)
        {
            ConnectionConfig = Configuration.Impls.Config.GetConfig<CxOneConnection>();
        }

        private CxOneConnection ConnectionConfig { get; set; }

        public override string DisplayName => "CheckmarxOne";

        private CxOneRestContext? Context { get; set; }


        private Task<CxProjects.ProjectIndex>? ProjectsFetchTask { get; set; }

        private Task<CxApplications.ApplicationIndex>? ApplicationsFetchTask { get; set; }


        private CxOneProjectScanEngineCount ScanEngineStats { get; set; } = new();


        private CxAudit.QueryDataMediator? QueryData { get; set; }

        private static String NormalizeEngineName(String name) => $"{name.ToUpper()}";

        private ConcurrentDictionary<String, Task<ProjectConfiguration>> ProjectConfigFetchTasks { get; set; } = new();

        private PredicateMediator? Predicates { get; set;}


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

            QueryData = new(Context, token);
            Predicates = new(Context, token);

            ThreadOpts.CancellationToken = token;

            _log.Debug("Starting CxOne transform");

            ProjectsFetchTask = CxProjects.GetProjects(Context, token);
            ApplicationsFetchTask = CxApplications.GetApplications(Context, token);

            using (var groupsTask = CxGroups.GetGroups(Context, token))
            using (var lastScansTask = CxProjects.GetProjectLatestCompletedScans(Context, token))
            {

                _log.Debug("Resolving projects.");
                var projectDescriptors = new ConcurrentBag<ProjectDescriptor>();

                Parallel.ForEach(ProjectsFetchTask.Result, ThreadOpts, (p) =>
                {
                    ProjectConfigFetchTasks.TryAdd(p.ProjectId, CxConfiguration.GetProjectConfiguration(Context, ThreadOpts.CancellationToken, p.ProjectId));
                    String groupsString = String.Join(",", p.Groups.ConvertAll((groupId) => groupsTask.Result[groupId].Path));

                    // Projects don't need to have a team assignment, unlike in SAST
                    if (!((p.Groups.Count == 0) ? Filter.Matches("", p.ProjectName)
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
                            TeamName = groupsString,
                            PresetName = ProjectConfigFetchTasks[p.ProjectId].Result.Preset

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
                        using (var scanCollection = CxScans.GetCompletedScans(Context, ThreadOpts.CancellationToken, projDescriptor.ProjectId))
                        {
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

                    using (var sca_risk_states = CxScanResults.GetScaRiskStates(Context, ThreadOpts.CancellationToken, project.ProjectId))
                    {

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

                            using (var rpt = CxScanResults.GetScanResults(Context, ThreadOpts.CancellationToken, scan.ScanId))
                                {

                                    if (rpt.Result.SastResults != null && rpt.Result.SastResults.Count > 0)
                                        OutputSastScanResults(scanTrx, project, scan, loadedScans, rpt.Result.SastResults, scanMetadata.Result[scan.ScanId]);

                                    if (rpt.Result.ScaResults != null && rpt.Result.ScaResults.Count > 0)
                                        OutputScaScanResults(scanTrx, project, scan, loadedScans, rpt.Result.ScaResults, sca_risk_states.Result);

                                    if (!ThreadOpts.CancellationToken.IsCancellationRequested && scanTrx.Commit())
                                        State.ScanCompleted(scan);
                                }
                            }
                        }
                    }
                });
            }
        }

        protected override void AddAdditionalProjectInfo(IDictionary<string, object> here, string projectId)
        {
            base.AddAdditionalProjectInfo(here, projectId);


            if (ProjectsFetchTask != null)
            {
                here.Add("ProjectCreated", ProjectsFetchTask.Result[projectId].Created);
                here.Add("ProjectUpdated", ProjectsFetchTask.Result[projectId].Updated);
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

        private void OutputScaScanResults(IOutputTransaction scanTrx, ProjectDescriptor project, ScanDescriptor scan, 
            CxScans.ScanIndex scanHeaders, List<CxScanResults.ScaResult> scaResults, IndexedRiskStates riskStates)
        {
            using (var detailed_report = CxScanResults.GetScaScanResults(Context, ThreadOpts.CancellationToken, scan.ScanId))
            {
                var flat_summary = new SortedDictionary<String, Object>();
                AddScanHeaderElements(scan, flat_summary);
                AddCommonScanFields(scan, scanHeaders, flat_summary);
                AddPairsAsTags(scanHeaders[scan.ScanId].Tags, flat_summary);
                ScaTransformer.Transformer.FillScanSummaryData(detailed_report.Result, flat_summary, scan.Project.ProjectName);
                scanTrx.write(ScaScanSummaryOut, flat_summary);

                var detail_header = new SortedDictionary<String, Object>();
                AddScanHeaderElements(scan, detail_header);
                AddCommonScanFields(scan, scanHeaders, detail_header);
                foreach (var flat_details in ScaTransformer.Transformer.GenerateScanDetailData(detailed_report.Result, detail_header, scan, riskStates))
                    scanTrx.write(ScaScanDetailOut, flat_details);
            }
        }

        private void OutputSastScanResults(IOutputTransaction scanTrx, ProjectDescriptor project, ScanDescriptor scan, CxScans.ScanIndex scanHeaders, 
            List<CxScanResults.SastResult> sastResults, CxSastScanMetadata.SastScanMetadata metadata)
        {
            using (var metrics = CxSastScanMetadata.GetScanMetrics(Context, ThreadOpts.CancellationToken, scan.ScanId))
            {
                var flat_summary = new SortedDictionary<String, object>();
                var flat_details_header = new SortedDictionary<String, object>();

                int low = 0, medium = 0, high = 0, info = 0, result_count = 0;

                MapBasicScanSummaryData(project, scan, scanHeaders, metadata, metrics, flat_summary);


                AddScanHeaderElements(scan, flat_details_header);

                foreach (var detail_entry in sastResults)
                {
                    result_count++;
                    var flat_details = new SortedDictionary<String, object>(flat_details_header);

                    flat_details.Add("SimilarityId", detail_entry.SimilarityId);
                    flat_details.Add("ResultSeverity", detail_entry.ResultSeverity);

                    if (Predicates != null)
                        flat_details.Add("Remark", String.Join(";", Predicates.GetComments(project.ProjectId, detail_entry.SimilarityId)));

                    if (detail_entry.State.CompareTo(NE_VALUE) != 0)
                    {
                        low += (detail_entry.ResultSeverity.ToLower().CompareTo("low") == 0) ? (1) : (0);
                        medium += (detail_entry.ResultSeverity.ToLower().CompareTo("medium") == 0) ? (1) : (0);
                        high += (detail_entry.ResultSeverity.ToLower().CompareTo("high") == 0) ? (1) : (0);
                        info += (detail_entry.ResultSeverity.ToLower().CompareTo("info") == 0) ? (1) : (0);
                    }

                    if (detail_entry.FirstFoundDate != DateTime.MinValue)
                        flat_details.Add("FirstDetectionDate", detail_entry.FirstFoundDate);

                    flat_details.Add("ResultId", detail_entry.ResultId);
                    flat_details.Add("State", detail_entry.State);
                    flat_details.Add("Status", detail_entry.Status);
                    flat_details.Add("QueryCweId", detail_entry.VulnerabilityDetails.CweId);
                    flat_details.Add("QueryCategories", String.Join(",", detail_entry.VulnerabilityDetails.Categories));
                    flat_details.Add("FalsePositive", detail_entry.State.CompareTo(NE_VALUE) == 0);
                    flat_details.Add("Branch", scanHeaders[scan.ScanId].Branch);
                    flat_details.Add("ScanFinished", scanHeaders[scan.ScanId].Updated);

                    if (QueryData != null)
                    {
                        var query_source = QueryData.GetQuerySource(project.ProjectId, detail_entry.Data);
                        var query = QueryData.GetQuery(project.ProjectId, detail_entry.Data);

                        flat_details.Add("QueryName", detail_entry.Data.QueryName);
                        flat_details.Add("QueryId", query.Id);
                        flat_details.Add("QueryLanguage", detail_entry.Data.LanguageName);
                        flat_details.Add("QueryGroup", detail_entry.Data.QueryGroup);
                        flat_details.Add("QuerySeverity", query_source.Severity.ToString());
                        flat_details.Add("QueryVersionCode", query_source.Modified);
                    }

                    flat_details.Add("VulnerabilityId", detail_entry.Data.ResultHash);

                    flat_details.Add("ResultDeepLink", UrlUtils.MakeUrl(UrlUtils.MakeUrl(ConnectionConfig.URL,
                        "results", scan.ScanId, project.ProjectId, "sast"),
                        new Dictionary<String, String> { { "result-id", HttpUtility.UrlEncode(detail_entry.Data.ResultHash) } }));


                    var node_cache = new List<SortedDictionary<String, object>>();

                    String sink_col = String.Empty;
                    String sink_line = String.Empty;
                    String sink_file = String.Empty;

                    int node_index = 0;
                    foreach (var node in detail_entry.Data.Flow)
                    {
                        var flat_node = new SortedDictionary<String, object>(flat_details);
                        ;

                        sink_col = node.NodeColumn;
                        sink_line = node.NodeLine;
                        sink_file = node.NodeFileName;

                        flat_node.Add("NodeColumn", node.NodeColumn);
                        flat_node.Add("NodeFileName", node.NodeFileName);
                        flat_node.Add("NodeLine", node.NodeLine);
                        flat_node.Add("NodeLength", node.NodeLength);
                        flat_node.Add("NodeType", node.NodeType);
                        flat_node.Add("NodeName", node.NodeFileName);
                        flat_node.Add("NodeId", ++node_index);

                        node_cache.Add(flat_node);
                    }


                    foreach (var entry in node_cache)
                    {
                        entry.Add("SinkColumn", sink_col);
                        entry.Add("SinkFileName", sink_file);
                        entry.Add("SinkLine", sink_line);
                        scanTrx.write(SastScanDetailOut, entry);
                    }
                }

                flat_summary.Add("Information", info);
                flat_summary.Add("Low", low);
                flat_summary.Add("Medium", medium);
                flat_summary.Add("High", high);

                scanTrx.write(SastScanSummaryOut, flat_summary);

                OutputScanStatistics(scanTrx, scan, metadata, metrics, ProjectConfigFetchTasks[project.ProjectId].Result, result_count);
            }
        }

        private void OutputScanStatistics(IOutputTransaction scanTrx, ScanDescriptor scan, CxSastScanMetadata.SastScanMetadata metadata, 
            Task<CxSastScanMetadata.SastScanMetrics> metrics, ProjectConfiguration project_config, int result_count)
        {
            var statistics = new SortedDictionary<String, object>();
            AddScanHeaderElements(scan, statistics);


            statistics.Add("FilteredParsedLOC", metadata.LOC);
            statistics.Add("UnfilteredParsedLOC", metrics.Result.ParsedLOCByLanguage.Sum((kv) => Convert.ToUInt32(kv.Value)));
            statistics.Add("PhysicalMemoryPeakMB", metrics.Result.MemoryPeak);
            statistics.Add("VirtualMemoryPeakMB", metrics.Result.VirtualMemoryPeak);
            statistics.Add("ResultCount", result_count);
            statistics.Add("FileFilter", project_config.SastFileFilter);

            var good_scan_lang_keys = metrics.Result.ParsedLOCByLanguage != null ? metrics.Result.ParsedLOCByLanguage.Keys.AsEnumerable() : new List<String> { };
            var bad_scan_lang_keys = metrics.Result.ParseFailureLOCByLanguage != null ? metrics.Result.ParseFailureLOCByLanguage.Keys.AsEnumerable() : new List<String> { };

            var lang_keys = good_scan_lang_keys.Union(bad_scan_lang_keys);

            Dictionary<String, Tuple<ulong, ulong>> langLOC_GoodBad = new(lang_keys.AsGenerator((lang) => KeyValuePair.Create(lang, new Tuple<ulong, ulong>(
                    (metrics.Result.ParsedLOCByLanguage != null && metrics.Result.ParsedLOCByLanguage.ContainsKey(lang)) ?
                    (metrics.Result.ParsedLOCByLanguage[lang]) : (0),
                    (metrics.Result.ParseFailureLOCByLanguage != null && metrics.Result.ParseFailureLOCByLanguage.ContainsKey(lang)) ?
                    (metrics.Result.ParseFailureLOCByLanguage[lang]) : (0)))));


            ulong unscanned_files = 0;

            foreach (var lang in lang_keys)
            {
                if (langLOC_GoodBad[lang].Item1 > 0 || langLOC_GoodBad[lang].Item2 > 0)
                {
                    statistics.Add($"{lang}_LOCParseSuccessPercent",
                        Convert.ToUInt32((langLOC_GoodBad[lang].Item1 / (double)((langLOC_GoodBad[lang].Item1 + langLOC_GoodBad[lang].Item2)) * 100.0)));
                    statistics.Add($"{lang}_LOCParseFailCount", langLOC_GoodBad[lang].Item2);
                    statistics.Add($"{lang}_LOCParsedCount", langLOC_GoodBad[lang].Item2);
                }

                if (metrics.Result.DomObjectsByLanguage != null && metrics.Result.DomObjectsByLanguage.ContainsKey(lang))
                    statistics.Add($"{lang}_DomObjectCount", metrics.Result.DomObjectsByLanguage[lang]);

                if (metrics.Result.UnscannedFilesByLanguage != null && metrics.Result.UnscannedFilesByLanguage.ContainsKey(lang))
                {
                    statistics.Add($"{lang}_FilesNotScanned", metrics.Result.UnscannedFilesByLanguage[lang]);
                    unscanned_files += metrics.Result.UnscannedFilesByLanguage[lang];
                }

                if (metrics.Result.ScannedFilesByLanguage != null && metrics.Result.ScannedFilesByLanguage.ContainsKey(lang))
                {
                    statistics.Add($"{lang}_FilesParsedSuccessfullyCount", metrics.Result.ScannedFilesByLanguage[lang].GoodFiles);
                    statistics.Add($"{lang}_FilesParseFailureCount", metrics.Result.ScannedFilesByLanguage[lang].BadFiles);
                    statistics.Add($"{lang}_FilesParsedPartiallyCount", metrics.Result.ScannedFilesByLanguage[lang].PartiallyGoodFiles);
                }
            }

            statistics.Add("UnscannedFileCount", unscanned_files);

            scanTrx.write(ScanStatisticsOut, statistics);
        }

        private void MapBasicScanSummaryData(ProjectDescriptor project, ScanDescriptor scan, CxScans.ScanIndex scanHeaders, 
            CxSastScanMetadata.SastScanMetadata metadata, Task<CxSastScanMetadata.SastScanMetrics> metrics, SortedDictionary<string, object> flat_summary)
        {
            AddScanHeaderElements(scan, flat_summary);
            flat_summary.Add("DeepLink", UrlUtils.MakeUrl(ConnectionConfig.URL, "results", scan.ScanId, project.ProjectId, "sast"));
            flat_summary.Add("SourceOrigin", scanHeaders[scan.ScanId].SourceOrigin);
            flat_summary.Add("SourceType", scanHeaders[scan.ScanId].SourceType);

            AddPairsAsTags(scanHeaders[scan.ScanId].Tags, flat_summary);

            AddCommonScanFields(scan, scanHeaders, flat_summary);

            flat_summary.Add("Preset", metadata.Preset);
            flat_summary.Add("LinesOfCode", metadata.LOC);
            flat_summary.Add("FileCount", metadata.FileCount);

            flat_summary.Add("Languages", String.Join(";", metrics.Result.ScannedFilesByLanguage.Keys));
            flat_summary.Add("FailedLinesOfCode",
                metrics.Result.ParseFailureLOCByLanguage.Count > 0 ? metrics.Result.ParseFailureLOCByLanguage.Values.Aggregate((x, y) => x + y) : 0);
        }

        private static void AddCommonScanFields(ScanDescriptor scan, CxScans.ScanIndex scanHeaders, SortedDictionary<string, object> flat_summary)
        {
            flat_summary.Add("Branch", scanHeaders[scan.ScanId].Branch);
            flat_summary.Add("ScanStart", scanHeaders[scan.ScanId].Created);
            flat_summary.Add("ScanFinished", scanHeaders[scan.ScanId].Updated);
            flat_summary.Add("Engines", scanHeaders[scan.ScanId].EnginesAsString);
            flat_summary.Add("Initiator", scanHeaders[scan.ScanId].Initiator);

            var scanTime = scanHeaders[scan.ScanId].Updated - scanHeaders[scan.ScanId].Created;
            flat_summary.Add("ScanTime", scanTime.ToString(@"hh\h\:mm\m\:ss\s"));
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

        public override void Dispose()
        {
            if (QueryData != null)
                QueryData.Dispose();

            if (Predicates != null)
                Predicates.Dispose();

            if (ProjectsFetchTask != null)
            {
                ProjectsFetchTask.Wait(ConnectionConfig.TimeoutSeconds * 1000);
                if (ProjectsFetchTask.IsCompleted)
                {
                    ProjectsFetchTask.Dispose();
                    ProjectsFetchTask = null;
                }
            }

            if (ApplicationsFetchTask != null)
            {
                ApplicationsFetchTask.Wait(ConnectionConfig.TimeoutSeconds * 1000);
                if (ApplicationsFetchTask.IsCompleted)
                {
                    ApplicationsFetchTask.Dispose();
                    ApplicationsFetchTask = null;
                }
            }

            foreach (var config_task in ProjectConfigFetchTasks.Values)
            {
                config_task.Wait(ConnectionConfig.TimeoutSeconds * 1000);
                if (config_task.IsCompleted)
                    config_task.Dispose();
            }
        }
    }
}