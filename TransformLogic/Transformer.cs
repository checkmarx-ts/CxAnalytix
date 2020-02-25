using CxRestClient;
using log4net;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Text;

namespace CxAnalytics.TransformLogic
{
    /// <summary>
    /// A class that implements the data transformation.
    /// </summary>
    public class Transformer
    {
        private static readonly String KEY_SCANID = "ScanId";
        private static readonly String KEY_SCANPRODUCT = "ScanProduct";
        private static readonly String KEY_SCANTYPE = "ScanType";
        private static readonly String KEY_SCANFINISH = "ScanFinished";
        private static readonly String KEY_SCANSTART = "ScanStart";
        private static readonly String KEY_SCANRISK = "ScanRisk";
        private static readonly String KEY_SCANRISKSEV = "ScanRiskSeverity";
        private static readonly String KEY_LOC = "LinesOfCode";
        private static readonly String KEY_FLOC = "FailedLinesOfCode";
        private static readonly String KEY_FILECOUNT = "FileCount";
        private static readonly String KEY_VERSION = "CxVersion";
        private static readonly String KEY_LANGS = "Languages";
        private static readonly String KEY_PRESET = "Preset";
        private static readonly String KEY_PROJECTID = "ProjectId";
        private static readonly String KEY_PROJECTNAME = "ProjectName";
        private static readonly String KEY_TEAMNAME = "TeamName";

        private static ILog _log = LogManager.GetLogger(typeof(Transformer));

        private static readonly String DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzz";
        /// <summary>
        /// The main logic for invoking a transformation.  It does not return until a sweep
        /// for new scans is performed across all projects.
        /// </summary>
        /// <param name="concurrentThreads">The number of concurrent scan transformation threads.</param>
        /// <param name="previousStatePath">A folder path where files will be created to store any state
        /// data required to resume operations across program runs.</param>
        /// <param name="ctx"></param>
        /// <param name="outFactory">The factory implementation for making IOutput instances
        /// used for outputting various record types.</param>
        /// <param name="records">The names of the supported record types that will be used by 
        /// the IOutputFactory to create the correct output implementation instance.</param>
        /// <param name="token">A cancellation token that can be used to stop processing of data if
        /// the task needs to be interrupted.</param>
        public static void doTransform(int concurrentThreads, String previousStatePath,
            CxRestContext ctx, IOutputFactory outFactory, RecordNames records, CancellationToken token)
        {
            ParallelOptions threadOpts = new ParallelOptions()
            {
                CancellationToken = token,
                MaxDegreeOfParallelism = concurrentThreads
            };

            // Policies may not have data if M&O is not installed.
            var policies = new ProjectPolicyIndex(CxMnoPolicies.GetAllPolicies(ctx, token));

            var scansToProcess = GetListOfScansAndIndexPolicies(previousStatePath, ctx,
                token, policies);

            ConcurrentDictionary<int, ViolatedPolicyCollection> projectViolations =
                new ConcurrentDictionary<int, ViolatedPolicyCollection>();

            var project_info_out = outFactory.newInstance(records.ProjectInfo);
            var scan_summary_out = outFactory.newInstance(records.SASTScanSummary);
            var scan_detail_out = outFactory.newInstance(records.SASTScanDetail);
            var policy_violation_detail_out = outFactory.newInstance(records.PolicyViolations);

            Parallel.ForEach<ScanDescriptor>(scansToProcess.ScanDesciptors, threadOpts,
                (scan) =>
                {

                    _log.Debug($"Retrieving XML Report for scan {scan.ScanId}");
                    var report = CxSastXmlReport.GetXmlReport(ctx, token, scan.ScanId);
                    _log.Debug($"XML Report for scan {scan.ScanId} retrieved.");

                    _log.Debug($"Processing XML report for scan {scan.ScanId}");
                    ProcessReport(scan, report, scan_detail_out);
                    _log.Debug($"XML Report for scan {scan.ScanId} processed.");


                    if (projectViolations.TryAdd(scan.Project.ProjectId,
                        new ViolatedPolicyCollection()))
                    {
                        // Collect policy violations, only once per project
                        projectViolations[scan.Project.ProjectId] = CxMnoRetreivePolicyViolations.
                            GetViolations(ctx, token, scan.Project.ProjectId, policies);

                        OutputProjectInfoRecords(scan, project_info_out);
                    }


                    scan.IncrementPolicyViolations(projectViolations[scan.Project.ProjectId].
                        GetViolatedRulesByScanId(scan.ScanId));
                    OutputScanSummary(scan, scansToProcess, scan_summary_out);

                    OutputPolicyViolationDetails(scan, policies, projectViolations, policy_violation_detail_out);
                }

                );
        }

        private static void OutputPolicyViolationDetails(ScanDescriptor scan, 
            ProjectPolicyIndex policies, 
            ConcurrentDictionary<int, ViolatedPolicyCollection> projectViolations, 
            IOutput policy_violation_detail_out)
        {
            SortedDictionary<String, String> header = new SortedDictionary<string, string>();
            AddPrimaryKeyElements(scan, header);
            header.Add(KEY_SCANID, scan.ScanId);
            header.Add(KEY_SCANPRODUCT, scan.ScanProduct);
            header.Add(KEY_SCANTYPE, scan.ScanType);

            var violatedRules = projectViolations[scan.Project.ProjectId].
                GetViolatedRulesByScanId(scan.ScanId);

            if (violatedRules != null)
                foreach (var rule in violatedRules)
                {
                    SortedDictionary<String, String> flat = new SortedDictionary<string, string>(header);
                    flat.Add("PolicyId", Convert.ToString(rule.PolicyId));
                    flat.Add("PolicyName", policies.GetPolicyById(rule.PolicyId).Name);
                    flat.Add("RuleId", Convert.ToString(rule.RuleId));
                    flat.Add("RuleName", rule.Name);
                    flat.Add("RuleDescription", rule.Description);
                    flat.Add("RuleType", rule.RuleType);
                    flat.Add("RuleCreateDate", rule.CreatedOn.ToString(DATE_FORMAT));
                    flat.Add("FirstViolationDetectionDate", rule.FirstDetectionDate.ToString(DATE_FORMAT));
                    flat.Add("ViolationName", rule.ViolationName);
                    if (rule.ViolationOccured.HasValue)
                        flat.Add("ViolationOccurredDate", rule.ViolationOccured.Value.ToString(DATE_FORMAT));
                    if (rule.ViolationRiskScore.HasValue)
                        flat.Add("ViolationRiskScore", Convert.ToString(rule.ViolationRiskScore.Value));
                    flat.Add("ViolationSeverity", rule.ViolationSeverity);
                    if (rule.ViolationSource != null)
                        flat.Add("ViolationSource", rule.ViolationSource);
                    flat.Add("ViolationState", rule.ViolationState);
                    flat.Add("ViolationStatus", rule.ViolationStatus);
                    if (rule.ViolationType != null)
                        flat.Add("ViolationType", rule.ViolationType);

                    policy_violation_detail_out.write(flat);
                }
        }

        private static void ProcessReport(ScanDescriptor scan, Stream report,
            IOutput scanDetailOut)
        {
            SortedDictionary<String, String> reportRec =
                new SortedDictionary<string, string>();
            AddPrimaryKeyElements(scan, reportRec);
            reportRec.Add(KEY_SCANID, scan.ScanId);
            reportRec.Add(KEY_SCANPRODUCT, scan.ScanProduct);
            reportRec.Add(KEY_SCANTYPE, scan.ScanType);

            SortedDictionary<String, String> curResultRec = null;
            SortedDictionary<String, String> curQueryRec = null;
            SortedDictionary<String, String> curPath = null;
            SortedDictionary<String, String> curPathNode = null;
            bool inSnippet = false;

            using (XmlReader xr = XmlReader.Create(report))
                while (xr.Read())
                {
                    if (xr.NodeType == XmlNodeType.Element)
                    {
                        if (xr.Name.CompareTo("CxXMLResults") == 0)
                        {
                            _log.Debug($"[Scan: {scan.ScanId}] Processing attributes in CxXMLResults.");

                            scan.Preset = xr.GetAttribute("Preset");
                            scan.Initiator = xr.GetAttribute("InitiatorName");
                            scan.DeepLink = xr.GetAttribute("DeepLink");
                            scan.ScanTime = xr.GetAttribute("ScanTime");
                            scan.ReportCreateTime = DateTime.Parse(xr.GetAttribute
                                ("ReportCreationTime"));
                            scan.Comments = xr.GetAttribute("ScanComments");
                            scan.SourceOrigin = xr.GetAttribute("SourceOrigin");
                            continue;
                        }

                        if (xr.Name.CompareTo("Query") == 0)
                        {
                            _log.Debug($"[Scan: {scan.ScanId}] Processing attributes in Query " +
                                $"[{xr.GetAttribute("id")} - {xr.GetAttribute("name")}].");

                            curQueryRec = new SortedDictionary<string, string>
                                (reportRec);

                            curQueryRec.Add("QueryCategories", xr.GetAttribute("categories"));
                            curQueryRec.Add("QueryId", xr.GetAttribute("id"));
                            curQueryRec.Add("QueryCweId", xr.GetAttribute("cweId"));
                            curQueryRec.Add("QueryName", xr.GetAttribute("name"));
                            curQueryRec.Add("QueryGroup", xr.GetAttribute("group"));
                            curQueryRec.Add("QuerySeverity", xr.GetAttribute("Severity"));
                            curQueryRec.Add("QueryLanguage", xr.GetAttribute("Language"));
                            curQueryRec.Add("QueryVersionCode", xr.GetAttribute("QueryVersionCode"));
                            continue;
                        }

                        if (xr.Name.CompareTo("Result") == 0)
                        {
                            _log.Debug($"[Scan: {scan.ScanId}] Processing attributes in Result " +
                                $"[{xr.GetAttribute("NodeId")}].");

                            scan.IncrementSeverity(xr.GetAttribute("Severity"));

                            curResultRec = new SortedDictionary<string, string>(curQueryRec);
                            curResultRec.Add("VulnerabilityId", xr.GetAttribute("NodeId"));
                            curResultRec.Add("SinkFileName", xr.GetAttribute("FileName"));
                            curResultRec.Add("Status", xr.GetAttribute("Status"));
                            curResultRec.Add("SinkLine", xr.GetAttribute("Line"));
                            curResultRec.Add("SinkColumn", xr.GetAttribute("Column"));
                            curResultRec.Add("FalsePositive", xr.GetAttribute("FalsePositive"));
                            curResultRec.Add("ResultSeverity", xr.GetAttribute("Severity"));
                            // TODO: Translate state number to an appropriate string
                            curResultRec.Add("State", xr.GetAttribute("state"));
                            curResultRec.Add("Remark", xr.GetAttribute("Remark"));
                            curResultRec.Add("ResultDeepLink", xr.GetAttribute("DeepLink"));
                            continue;
                        }

                        if (xr.Name.CompareTo("Path") == 0)
                        {
                            curPath = new SortedDictionary<string, string>(curResultRec);
                            curPath.Add("ResultId", xr.GetAttribute("ResultId"));
                            curPath.Add("PathId", xr.GetAttribute("PathId"));
                            curPath.Add("SimilarityId", xr.GetAttribute("SimilarityId"));
                            continue;
                        }

                        if (xr.Name.CompareTo("PathNode") == 0)
                        {
                            curPathNode = new SortedDictionary<string, string>(curPath);
                            continue;
                        }

                        if (xr.Name.CompareTo("FileName") == 0 && curPathNode != null)
                        {
                            curPathNode.Add("NodeFileName", xr.ReadElementContentAsString());
                            continue;
                        }

                        if (xr.Name.CompareTo("Line") == 0 && curPathNode != null && !inSnippet)
                        {
                            curPathNode.Add("NodeLine", xr.ReadElementContentAsString());
                            continue;
                        }

                        if (xr.Name.CompareTo("Column") == 0 && curPathNode != null)
                        {
                            curPathNode.Add("NodeColumn", xr.ReadElementContentAsString());
                            continue;
                        }

                        if (xr.Name.CompareTo("NodeId") == 0 && curPathNode != null)
                        {
                            curPathNode.Add("NodeId", xr.ReadElementContentAsString());
                            continue;
                        }

                        if (xr.Name.CompareTo("Name") == 0 && curPathNode != null)
                        {
                            curPathNode.Add("NodeName", xr.ReadElementContentAsString());
                            continue;
                        }

                        if (xr.Name.CompareTo("Type") == 0 && curPathNode != null)
                        {
                            curPathNode.Add("NodeType", xr.ReadElementContentAsString());
                            continue;
                        }

                        if (xr.Name.CompareTo("Length") == 0 && curPathNode != null)
                        {
                            curPathNode.Add("NodeLength", xr.ReadElementContentAsString());
                            continue;
                        }

                        if (xr.Name.CompareTo("Snippet") == 0 && curPathNode != null)
                        {
                            inSnippet = true;
                            continue;
                        }

                        if (xr.Name.CompareTo("Code") == 0 && curPathNode != null)
                        {
                            curPathNode.Add("NodeCodeSnippet", xr.ReadElementContentAsString());
                            continue;
                        }
                    }


                    if (xr.NodeType == XmlNodeType.EndElement)
                    {
                        if (xr.Name.CompareTo("CxXMLResults") == 0)
                        {
                            _log.Debug($"[Scan: {scan.ScanId}] Finished processing CxXMLResults");
                            continue;
                        }

                        if (xr.Name.CompareTo("Query") == 0)
                        {
                            curQueryRec = null;
                            continue;
                        }

                        if (xr.Name.CompareTo("Result") == 0)
                        {
                            curResultRec = null;
                            continue;
                        }

                        if (xr.Name.CompareTo("Path") == 0)
                        {
                            curPath = null;
                            continue;
                        }

                        if (xr.Name.CompareTo("PathNode") == 0)
                        {
                            scanDetailOut.write(curPathNode);
                            curPathNode = null;
                            continue;
                        }

                        if (xr.Name.CompareTo("PathNode") == 0)
                        {
                            inSnippet = false;
                            continue;
                        }
                    }
                }
        }

        private static void OutputScanSummary(ScanDescriptor scanRecord, ScanData scansToProcess,
            IOutput scan_summary_out)
        {
            SortedDictionary<String, String> flat = new SortedDictionary<string, string>();
            AddPrimaryKeyElements(scanRecord, flat);
            flat.Add(KEY_SCANID, scanRecord.ScanId);
            flat.Add(KEY_SCANPRODUCT, scanRecord.ScanProduct);
            flat.Add(KEY_SCANTYPE, scanRecord.ScanType);
            flat.Add(KEY_SCANFINISH, scanRecord.FinishedStamp.ToString(DATE_FORMAT));
            flat.Add(KEY_SCANSTART, scansToProcess.ScanDataDetails[scanRecord.ScanId].StartTime.ToString(DATE_FORMAT));
            flat.Add(KEY_SCANRISK, scansToProcess.ScanDataDetails[scanRecord.ScanId].ScanRisk.ToString());
            flat.Add(KEY_SCANRISKSEV, scansToProcess.ScanDataDetails[scanRecord.ScanId].ScanRiskSeverity.ToString());
            flat.Add(KEY_LOC, scansToProcess.ScanDataDetails[scanRecord.ScanId].LinesOfCode.ToString());
            flat.Add(KEY_FLOC, scansToProcess.ScanDataDetails[scanRecord.ScanId].FailedLinesOfCode.ToString());
            flat.Add(KEY_FILECOUNT, scansToProcess.ScanDataDetails[scanRecord.ScanId].FileCount.ToString());
            flat.Add(KEY_VERSION, scansToProcess.ScanDataDetails[scanRecord.ScanId].CxVersion);
            flat.Add(KEY_LANGS, scansToProcess.ScanDataDetails[scanRecord.ScanId].Languages);
            flat.Add(KEY_PRESET, scanRecord.Preset);
            flat.Add("Initiator", scanRecord.Initiator);
            flat.Add("DeepLink", scanRecord.DeepLink);
            flat.Add("ScanTime", scanRecord.ScanTime);
            flat.Add("ReportCreationTime", scanRecord.ReportCreateTime.ToString(DATE_FORMAT));
            flat.Add("ScanComments", scanRecord.Comments);
            flat.Add("SourceOrigin", scanRecord.SourceOrigin);
            foreach (var sev in scanRecord.SeverityCounts.Keys)
                flat.Add(sev, Convert.ToString(scanRecord.SeverityCounts[sev]));

            if (scanRecord.HasPoliciesApplied)
            {
                flat.Add("PoliciesViolated", Convert.ToString(scanRecord.PoliciesViolated));
                flat.Add("RulesViolated", Convert.ToString(scanRecord.RulesViolated));
                flat.Add("PolicyViolations", Convert.ToString(scanRecord.Violations));
            }

            scan_summary_out.write(flat);
        }

        private static void OutputProjectInfoRecords(ScanDescriptor scanRecord,
            IOutput project_info_out)
        {
            SortedDictionary<String, String> flat = new SortedDictionary<string, string>();
            AddPrimaryKeyElements(scanRecord, flat);

            flat.Add(KEY_PRESET, scanRecord.Project.PresetName);
            flat.Add("Policies", scanRecord.Project.Policies);

            foreach (var lastScanProduct in scanRecord.Project.LatestScanDateByProduct.Keys)
                flat.Add($"{lastScanProduct}_LastScanDate",
                    scanRecord.Project.LatestScanDateByProduct[lastScanProduct].ToString(DATE_FORMAT));

            foreach (var scanCountProduct in scanRecord.Project.ScanCountByProduct.Keys)
                flat.Add($"{scanCountProduct}_Scans",
                    scanRecord.Project.ScanCountByProduct[scanCountProduct].ToString());

            project_info_out.write(flat);

        }

        private static void AddPrimaryKeyElements(ScanDescriptor rec, IDictionary<string, string> flat)
        {
            flat.Add(KEY_PROJECTID, rec.Project.ProjectId.ToString());
            flat.Add(KEY_PROJECTNAME, rec.Project.ProjectName);
            flat.Add(KEY_TEAMNAME, rec.Project.TeamName);
        }

        private class ScanData
        {
            public ScanData()
            {
                ScanDataDetails = new Dictionary<string, CxSastScans.Scan>();
            }

            public IEnumerable<ScanDescriptor> ScanDesciptors { get; set; }
            public Dictionary<String, CxSastScans.Scan> ScanDataDetails { get; private set; }
        }


        private static String GetFlatPolicyNames(PolicyCollection policies,
            IEnumerable<int> policyIds)
        {
            StringBuilder b = new StringBuilder();

            foreach (var pid in policyIds)
            {
                if (b.Length > 0)
                    b.Append(';');

                b.Append(policies.GetPolicyById(pid).Name);
            }

            return b.ToString();
        }


        private static ScanData GetListOfScansAndIndexPolicies(string previousStatePath, CxRestContext ctx,
            CancellationToken token, ProjectPolicyIndex index)
        {
            DateTime checkStart = DateTime.Now;

            // Populate the data resolver with teams and presets
            DataResolver dr = new DataResolver();

            var presetEnum = CxPresets.GetPresets(ctx, token);

            foreach (var preset in presetEnum)
                dr.addPreset(preset.PresetId, preset.PresetName);

            var teamEnum = CxTeams.GetTeams(ctx, token);

            foreach (var team in teamEnum)
                dr.addTeam(team.TeamId, team.TeamName);

            // Now populate the project resolver with the projects
            ProjectResolver pr = dr.Resolve(previousStatePath);

            var projects = CxProjects.GetProjects(ctx, token);

            foreach (var p in projects)
            {
                IEnumerable<int> projectPolicyList = CxMnoPolicies.GetPolicyIdsForProject
                    (ctx, token, p.ProjectId);

                index.CorrelateProjectToPolicies(p.ProjectId, projectPolicyList);

                String policies = GetFlatPolicyNames(index, projectPolicyList);

                pr.AddProject(p.TeamId, p.PresetId, p.ProjectId, p.ProjectName, policies);
            }


            // Resolve projects to get the scan resolver.
            ScanResolver sr = pr.Resolve();

            ScanData retVal = new ScanData();

            // Get SAST and SCA scans
            var sastScans = CxSastScans.GetScans(ctx, token, CxSastScans.ScanStatus.Finished);

            foreach (var sastScan in sastScans)
            {
                sr.addScan(sastScan.ProjectId, sastScan.ScanType, "SAST", sastScan.ScanId, sastScan.FinishTime);
                retVal.ScanDataDetails.Add(sastScan.ScanId, sastScan);
            }

            // TODO: SCA scans need to be loaded and added to the ScanResolver instance.


            // Get the scans to process, update the last check date in all projects.
            retVal.ScanDesciptors = sr.Resolve(checkStart);

            return retVal;
        }
    }
}
