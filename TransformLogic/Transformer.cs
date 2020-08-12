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
using CxAnalytix.TransformLogic.Data;

namespace CxAnalytix.TransformLogic
{
    /// <summary>
    /// A class that implements the data transformation.
    /// </summary>
    public class Transformer
    {
        private static ILog _log = LogManager.GetLogger(typeof(Transformer));

        private static readonly String SAST_PRODUCT_STRING = "SAST";
        private static readonly String SCA_PRODUCT_STRING = "SCA";

        private static readonly Dictionary<String, Action<ScanDescriptor, Transformer>> _mapActions;


        private IEnumerable<ScanDescriptor> ScanDescriptors { get; set; }

        private Dictionary<String, CxSastScans.Scan> SastScanCache { get; set; }
            = new Dictionary<string, CxSastScans.Scan>();
        private Dictionary<String, CxScaScans.Scan> ScaScanCache { get; set; }
            = new Dictionary<string, CxScaScans.Scan>();

        private ProjectPolicyIndex Policies { get; set; }
        private DateTime CheckTime { get; set; } = DateTime.Now;

        private CxRestContext RestContext { get; set; }
        private CancellationToken CancelToken { get; set; }

        ParallelOptions ThreadOpts { get; set; }

        public String InstanceId { get; set; }

        public IOutput ProjectInfoOut { get; internal set; }
        public IOutput SastScanSummaryOut { get; internal set; }
        public IOutput SastScanDetailOut { get; internal set; }
        public IOutput ScaScanSummaryOut { get; internal set; }
        public IOutput ScaScanDetailOut { get; internal set; }
        public IOutput PolicyViolationDetailOut { get; internal set; }

        private ConcurrentDictionary<int, ViolatedPolicyCollection> PolicyViolations { get; set; } =
            new ConcurrentDictionary<int, ViolatedPolicyCollection>();


        static Transformer()
        {
            _mapActions = new Dictionary<string, Action<ScanDescriptor, Transformer>>()
            {
                {SAST_PRODUCT_STRING,  SastReportOutput},
                {SCA_PRODUCT_STRING,  ScaReportOutput}
            };
        }

        public static void SastReportOutput(ScanDescriptor scan, Transformer inst)
        {

            _log.Debug($"Retrieving XML Report for scan {scan.ScanId}");
            try
            {
                using (var report = CxSastXmlReport.GetXmlReport(inst.RestContext, 
                    inst.CancelToken, scan.ScanId))
                {
                    _log.Debug($"XML Report for scan {scan.ScanId} retrieved.");

                    _log.Debug($"Processing XML report for scan {scan.ScanId}");
                    inst.ProcessSASTReport(scan, report);
                    _log.Debug($"XML Report for scan {scan.ScanId} processed.");
                }

                inst.OutputSASTScanSummary(scan);
            }
            catch (Exception ex)
            {
                _log.Warn($"Error attempting to retrieve the SAST XML report for {scan.ScanId}" +
                    $" in project {scan.Project.ProjectId}: {scan.Project.ProjectName}. ", ex);
            }
        }

        public static void ScaReportOutput(ScanDescriptor sd, Transformer inst)
        {
            Dictionary<String, CxScaLicenses.License> licenseIndex =
                new Dictionary<string, CxScaLicenses.License>();

            Dictionary<String, int> licenseCount =
                new Dictionary<string, int>();

            try
            {
                var licenses = CxScaLicenses.GetLicenses(inst.RestContext, inst.CancelToken, sd.ScanId);

                foreach (var l in licenses)
                {
                    licenseIndex.Add(l.LicenseId, l);

                    if (licenseCount.ContainsKey(l.RiskLevel))
                        licenseCount[l.RiskLevel]++;
                    else
                        licenseCount.Add(l.RiskLevel, 1);
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not obtain license data for scan {sd.ScanId} in project " +
                    $"{sd.Project.ProjectId}: {sd.Project.ProjectName}.  License data will not be" +
                    $" available.", ex);
            }

            Dictionary<String, CxScaLibraries.Library> libraryIndex =
            new Dictionary<string, CxScaLibraries.Library>();


            try
            {
                var libraries = CxScaLibraries.GetLibraries(inst.RestContext, inst.CancelToken, sd.ScanId);

                foreach (var lib in libraries)
                    libraryIndex.Add(lib.LibraryId, lib);
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not obtain library data for scan {sd.ScanId} in project " +
                    $"{sd.Project.ProjectId}: {sd.Project.ProjectName}.  Library data will not be" +
                    $" available.", ex);
            }

            OutputScaScanSummary(sd, inst, licenseCount);

            OutputScaScanDetails(sd, inst, licenseIndex, libraryIndex);

        }

        private static void OutputScaScanDetails(ScanDescriptor sd, Transformer inst, 
            Dictionary<string, CxScaLicenses.License> licenseIndex, 
            Dictionary<string, CxScaLibraries.Library> libraryIndex)
        {
            try
            {
                var vulns = CxScaVulnerabilities.GetVulnerabilities(inst.RestContext, 
                    inst.CancelToken, sd.ScanId);

                var header = new SortedDictionary<String, Object>();
                inst.AddPrimaryKeyElements(sd, header);
                header.Add(PropertyKeys.KEY_SCANFINISH, sd.FinishedStamp);

                foreach (var vuln in vulns)
                {
                    var flat = new SortedDictionary<String, Object>(header);

                    flat.Add(PropertyKeys.KEY_SCANID, sd.ScanId);

                    flat.Add("VulnerabilityId", vuln.VulerabilityId);
                    flat.Add(PropertyKeys.KEY_SIMILARITYID, vuln.SimilarityId);
                    flat.Add("CVEName", vuln.CVEName);
                    flat.Add("CVEDescription", vuln.CVEDescription);
                    flat.Add("CVEUrl", vuln.CVEUrl);
                    flat.Add("CVEPubDate", vuln.CVEPublishDate);
                    flat.Add("CVEScore", vuln.CVEScore);
                    flat.Add("Recommendation", vuln.Recommendations);
                    flat.Add(PropertyKeys.KEY_SCANRISKSEV, vuln.Severity.Name);
                    flat.Add("State", vuln.State.StateName);


                    flat.Add("LibraryId", vuln.LibraryId);

                    var lib = libraryIndex[vuln.LibraryId];
                    if (lib != null)
                    {
                        flat.Add("LibraryName", lib.LibraryName);
                        flat.Add("LibraryVersion", lib.LibraryVersion);
                        flat.Add("LibraryReleaseDate", lib.ReleaseDate);
                        flat.Add("LibraryLatestVersion", lib.LatestVersion);
                        flat.Add("LibraryLatestReleaseDate", lib.LatestVersionReleased);
                    }

                    StringBuilder licenseStr = new StringBuilder();

                    foreach (var license in lib.Licenses)
                    {
                        if (licenseStr.Length > 0)
                            licenseStr.Append(";");
                        licenseStr.Append(licenseIndex[license].LicenseName);

                        flat.Add($"LibraryLegalRisk_{licenseIndex[license].LicenseName.Replace(" ", "")}",
                            licenseIndex[license].RiskLevel);
                    }

                    flat.Add("LibraryLicenses", licenseStr.ToString());

                    inst.ScaScanDetailOut.write(flat);
                }
            }
            catch (Exception ex)
            {
                _log.Warn($"Could not obtain vulnerability data for scan {sd.ScanId} in project " +
                $"{sd.Project.ProjectId}: {sd.Project.ProjectName}.  Vulnerability data will not be" +
                $" available.", ex);
            }
        }

        private static void OutputScaScanSummary(ScanDescriptor sd, Transformer inst, Dictionary<string, int> licenseCount)
        {
            var flat = new SortedDictionary<String, Object>();
            inst.AddPrimaryKeyElements(sd, flat);
            AddPolicyViolationProperties(sd, flat);
            flat.Add(PropertyKeys.KEY_SCANID, sd.ScanId);
            flat.Add(PropertyKeys.KEY_SCANSTART, inst.ScaScanCache[sd.ScanId].StartTime);
            flat.Add(PropertyKeys.KEY_SCANFINISH, inst.ScaScanCache[sd.ScanId].FinishTime);

            foreach (var k in licenseCount.Keys)
                flat.Add($"Legal{k}", licenseCount[k]);


            try
            {
                var summary = CxScaSummaryReport.GetReport(inst.RestContext, inst.CancelToken, sd.ScanId);

                flat.Add("HighVulnerabilityLibraries", summary.HighVulnerabilityLibraries);
                flat.Add("LowVulnerabilityLibraries", summary.LowVulnerabilityLibraries);
                flat.Add("MediumVulnerabilityLibraries", summary.MediumVulnerabilityLibraries);
                flat.Add("NonVulnerableLibraries", summary.NonVulnerableLibraries);
                flat.Add("TotalHighVulnerabilities", summary.TotalHighVulnerabilities);
                flat.Add("TotalLibraries", summary.TotalLibraries);
                flat.Add("TotalLowVulnerabilities", summary.TotalLowVulnerabilities);
                flat.Add("TotalMediumVulnerabilities", summary.TotalMediumVulnerabilities);
                flat.Add("VulnerabilityScore", summary.VulnerabilityScore);
                flat.Add("VulnerableAndOutdated", summary.VulnerableAndOutdated);
                flat.Add("VulnerableAndUpdated", summary.VulnerableAndUpdated);
            }
            catch (Exception ex)
            {
                _log.Warn($"Error obtaining summary report for SCA scan {sd.ScanId} " +
                    $"in project {sd.Project.ProjectName}", ex);
            }

            inst.ScaScanSummaryOut.write(flat);
        }

        private Transformer(CxRestContext ctx, CancellationToken token,
        String previousStatePath)
        {
            RestContext = ctx;
            CancelToken = token;

            Policies = null;

            // Policies may not have data if M&O is not installed.
            try
            {
                Policies = new ProjectPolicyIndex(CxMnoPolicies.GetAllPolicies(ctx, token));
            }
            catch (Exception ex)
            {
                _log.Warn("Policy data is not available.", ex);
            }


            // Populate the data resolver with teams and presets
            DataResolver dr = new DataResolver();

            var presetEnum = CxPresets.GetPresets(RestContext, CancelToken);

            foreach (var preset in presetEnum)
                dr.addPreset(preset.PresetId, preset.PresetName);

            var teamEnum = CxTeams.GetTeams(RestContext, CancelToken);

            foreach (var team in teamEnum)
                dr.addTeam(team.TeamId, team.TeamName);

            // Now populate the project resolver with the projects
            ProjectResolver pr = dr.Resolve(previousStatePath);

            var projects = CxProjects.GetProjects(RestContext, CancelToken);

            foreach (var p in projects)
            {
                String combinedPolicyNames = String.Empty;

                if (Policies != null)
                {
                    try
                    {
                        IEnumerable<int> projectPolicyList = CxMnoPolicies.GetPolicyIdsForProject
                            (ctx, token, p.ProjectId);

                        Policies.CorrelateProjectToPolicies(p.ProjectId, projectPolicyList);
                        combinedPolicyNames = GetFlatPolicyNames(Policies, projectPolicyList);
                    }
                    catch (Exception ex)
                    {
                        _log.Warn($"Unable to correlate policies to project {p.ProjectId}: {p.ProjectName}. " +
                            $"Policy statistics will be unavalable.", ex);
                    }
                }

                var cfDict = new SortedDictionary<String, String>();
                p.CustomFields.ForEach((cf) => cfDict.Add (cf.FieldName, cf.FieldValue) );

                pr.AddProject(p.TeamId, p.PresetId, p.ProjectId, p.ProjectName, combinedPolicyNames, cfDict);
            }

            // Resolve projects to get the scan resolver.
            ScanResolver sr = pr.Resolve(_mapActions);

            try
            {
                var sastScans = CxSastScans.GetScans(RestContext, CancelToken, CxSastScans.ScanStatus.Finished);
                foreach (var sastScan in sastScans)
                {
                    sr.AddScan(sastScan.ProjectId, sastScan.ScanType, SAST_PRODUCT_STRING,
                        sastScan.ScanId, sastScan.FinishTime);

                    SastScanCache.Add(sastScan.ScanId, sastScan);
                }


                foreach (var p in projects)
                {
                    var scaScans = CxScaScans.GetScans(ctx, token, p.ProjectId);
                    foreach (var scaScan in scaScans)
                    {
                        sr.AddScan(scaScan.ProjectId, "Composition", SCA_PRODUCT_STRING, scaScan.ScanId,
                            scaScan.FinishTime);
                        ScaScanCache.Add(scaScan.ScanId, scaScan);
                    }
                }

                ScanDescriptors = sr.Resolve(CheckTime);
            }
            catch (Exception ex)
            {
                _log.Error($"Error resolving scans, server may be unavailable.", ex);
            }
        }


        private void ExecuteSweep()
        {
            // Lookup policy violations, report the project information records.
            Parallel.ForEach<ScanDescriptor>(ScanDescriptors, ThreadOpts,
            (scan) =>
            {
                if (PolicyViolations.TryAdd(scan.Project.ProjectId,
                new ViolatedPolicyCollection()))
                {
                    if (Policies != null)
                        try
                        {
                            // Collect policy violations, only once per project
                            PolicyViolations[scan.Project.ProjectId] = CxMnoRetreivePolicyViolations.
                            GetViolations(RestContext, CancelToken, scan.Project.ProjectId, Policies);
                        }
                        catch (Exception ex)
                        {
                            _log.Debug($"Policy violations for project {scan.Project.ProjectId}: " +
                            $"{scan.Project.ProjectName} are unavailable.", ex);
                        }

                    OutputProjectInfoRecords(scan);
                }

                // Increment the policy violation stats for each scan.
                scan.IncrementPolicyViolations(PolicyViolations[scan.Project.ProjectId].
                GetViolatedRulesByScanId(scan.ScanId));

                // Does something appropriate for the type of scan in the scan descriptor.
                scan.MapAction(scan, this);

                OutputPolicyViolationDetails(scan);
            });


        }



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
        public static void DoTransform(int concurrentThreads, String previousStatePath, String instanceId,
        CxRestContext ctx, IOutputFactory outFactory, RecordNames records, CancellationToken token)
        {

            Transformer xform = new Transformer(ctx, token, previousStatePath)
            {
                ThreadOpts = new ParallelOptions()
                {
                    CancellationToken = token,
                    MaxDegreeOfParallelism = concurrentThreads
                },
                ProjectInfoOut = outFactory.newInstance(records.ProjectInfo),
                SastScanSummaryOut = outFactory.newInstance(records.SASTScanSummary),
                SastScanDetailOut = outFactory.newInstance(records.SASTScanDetail),
                PolicyViolationDetailOut = outFactory.newInstance(records.PolicyViolations),
                ScaScanSummaryOut = outFactory.newInstance(records.SCAScanSummary),
                ScaScanDetailOut = outFactory.newInstance(records.SCAScanDetail),
                InstanceId = instanceId

            };

            xform.ExecuteSweep();
        }

        private void OutputPolicyViolationDetails(ScanDescriptor scan)
        {
            var header = new SortedDictionary<String, Object>();
            AddPrimaryKeyElements(scan, header);
            header.Add(PropertyKeys.KEY_SCANID, scan.ScanId);
            header.Add(PropertyKeys.KEY_SCANPRODUCT, scan.ScanProduct);
            header.Add(PropertyKeys.KEY_SCANTYPE, scan.ScanType);

            var violatedRules = PolicyViolations[scan.Project.ProjectId].
                GetViolatedRulesByScanId(scan.ScanId);

            if (violatedRules != null)
                foreach (var rule in violatedRules)
                {
                    var flat = new SortedDictionary<String, Object>(header);
                    flat.Add("PolicyId", rule.PolicyId);
                    flat.Add("PolicyName", Policies.GetPolicyById(rule.PolicyId).Name);
                    flat.Add("RuleId", rule.RuleId);
                    flat.Add("RuleName", rule.Name);
                    flat.Add("RuleDescription", rule.Description);
                    flat.Add("RuleType", rule.RuleType);
                    flat.Add("RuleCreateDate", rule.CreatedOn);
                    flat.Add("FirstViolationDetectionDate", rule.FirstDetectionDate);
                    flat.Add("ViolationName", rule.ViolationName);
                    if (rule.ViolationOccured.HasValue)
                        flat.Add("ViolationOccurredDate", rule.ViolationOccured.Value);
                    if (rule.ViolationRiskScore.HasValue)
                        flat.Add("ViolationRiskScore", rule.ViolationRiskScore.Value);
                    flat.Add("ViolationSeverity", rule.ViolationSeverity);
                    if (rule.ViolationSource != null)
                        flat.Add("ViolationSource", rule.ViolationSource);
                    flat.Add("ViolationState", rule.ViolationState);
                    flat.Add("ViolationStatus", rule.ViolationStatus);
                    if (rule.ViolationType != null)
                        flat.Add("ViolationType", rule.ViolationType);

                    PolicyViolationDetailOut.write(flat);
                }
        }

        private void ProcessSASTReport(ScanDescriptor scan, Stream report)
        {
            var reportRec = new SortedDictionary<String, Object>();
            AddPrimaryKeyElements(scan, reportRec);
            reportRec.Add(PropertyKeys.KEY_SCANID, scan.ScanId);
            reportRec.Add(PropertyKeys.KEY_SCANPRODUCT, scan.ScanProduct);
            reportRec.Add(PropertyKeys.KEY_SCANTYPE, scan.ScanType);
            reportRec.Add(PropertyKeys.KEY_SCANFINISH, scan.FinishedStamp);

            SortedDictionary<String, Object> curResultRec = null;
            SortedDictionary<String, Object> curQueryRec = null;
            SortedDictionary<String, Object> curPath = null;
            SortedDictionary<String, Object> curPathNode = null;
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
                            scan.ReportCreateTime = DateTime.Parse (xr.GetAttribute ("ReportCreationTime"));
                            scan.Comments = xr.GetAttribute("ScanComments");
                            scan.SourceOrigin = xr.GetAttribute("SourceOrigin");
                            continue;
                        }

                        if (xr.Name.CompareTo("Query") == 0)
                        {
                            _log.Debug($"[Scan: {scan.ScanId}] Processing attributes in Query " +
                                $"[{xr.GetAttribute("id")} - {xr.GetAttribute("name")}].");

                            curQueryRec = new SortedDictionary<String, Object>
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

                            curResultRec = new SortedDictionary<String, Object>(curQueryRec);
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
                            curPath = new SortedDictionary<String, Object>(curResultRec);
                            curPath.Add("ResultId", xr.GetAttribute("ResultId"));
                            curPath.Add("PathId", xr.GetAttribute("PathId"));
                            curPath.Add(PropertyKeys.KEY_SIMILARITYID, xr.GetAttribute("SimilarityId"));
                            continue;
                        }

                        if (xr.Name.CompareTo("PathNode") == 0)
                        {
                            curPathNode = new SortedDictionary<String, Object>(curPath);
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
                            SastScanDetailOut.write(curPathNode);
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

        private void OutputSASTScanSummary(ScanDescriptor scanRecord)
        {
            if (SastScanSummaryOut == null)
                return;

            var flat = new SortedDictionary<String, Object>();
            AddPrimaryKeyElements(scanRecord, flat);
            flat.Add(PropertyKeys.KEY_SCANID, scanRecord.ScanId);
            flat.Add(PropertyKeys.KEY_SCANPRODUCT, scanRecord.ScanProduct);
            flat.Add(PropertyKeys.KEY_SCANTYPE, scanRecord.ScanType);
            flat.Add(PropertyKeys.KEY_SCANFINISH, scanRecord.FinishedStamp);
            flat.Add(PropertyKeys.KEY_SCANSTART, SastScanCache[scanRecord.ScanId].StartTime);
            flat.Add(PropertyKeys.KEY_ENGINESTART, SastScanCache[scanRecord.ScanId].EngineStartTime);
            flat.Add(PropertyKeys.KEY_ENGINEFINISH, SastScanCache[scanRecord.ScanId].EngineFinishTime);
            flat.Add(PropertyKeys.KEY_SCANRISK, SastScanCache[scanRecord.ScanId].ScanRisk);
            flat.Add(PropertyKeys.KEY_SCANRISKSEV, SastScanCache[scanRecord.ScanId].ScanRiskSeverity);
            flat.Add("LinesOfCode", SastScanCache[scanRecord.ScanId].LinesOfCode);
            flat.Add("FailedLinesOfCode", SastScanCache[scanRecord.ScanId].FailedLinesOfCode);
            flat.Add("FileCount", SastScanCache[scanRecord.ScanId].FileCount);
            flat.Add("CxVersion", SastScanCache[scanRecord.ScanId].CxVersion);
            flat.Add("Languages", SastScanCache[scanRecord.ScanId].Languages);
            flat.Add(PropertyKeys.KEY_PRESET, scanRecord.Preset);
            flat.Add("Initiator", scanRecord.Initiator);
            flat.Add("DeepLink", scanRecord.DeepLink);
            flat.Add("ScanTime", scanRecord.ScanTime);
            flat.Add("ReportCreationTime", scanRecord.ReportCreateTime);
            flat.Add("ScanComments", scanRecord.Comments);
            flat.Add("SourceOrigin", scanRecord.SourceOrigin);
            foreach (var sev in scanRecord.SeverityCounts.Keys)
                flat.Add(sev, scanRecord.SeverityCounts[sev]);

            AddPolicyViolationProperties(scanRecord, flat);

            SastScanSummaryOut.write(flat);
        }

        private static void AddPolicyViolationProperties(ScanDescriptor scanRecord,
            IDictionary<String, Object> rec)
        {
            if (scanRecord.HasPoliciesApplied)
            {
                rec.Add("PoliciesViolated", scanRecord.PoliciesViolated);
                rec.Add("RulesViolated", scanRecord.RulesViolated);
                rec.Add("PolicyViolations", scanRecord.Violations);
            }
        }

        private void OutputProjectInfoRecords(ScanDescriptor scanRecord)
        {
            var flat = new SortedDictionary<String, Object>();
            AddPrimaryKeyElements(scanRecord, flat);

            flat.Add(PropertyKeys.KEY_PRESET, scanRecord.Project.PresetName);
            flat.Add("Policies", scanRecord.Project.Policies);

            foreach (var lastScanProduct in scanRecord.Project.LatestScanDateByProduct.Keys)
                flat.Add($"{lastScanProduct}_LastScanDate",
                    scanRecord.Project.LatestScanDateByProduct[lastScanProduct]);

            foreach (var scanCountProduct in scanRecord.Project.ScanCountByProduct.Keys)
                flat.Add($"{scanCountProduct}_Scans",
                    scanRecord.Project.ScanCountByProduct[scanCountProduct]);

            if (scanRecord.Project.CustomFields != null && scanRecord.Project.CustomFields.Count > 0)
                flat.Add(PropertyKeys.KEY_CUSTOMFIELDS, scanRecord.Project.CustomFields);

            ProjectInfoOut.write(flat);

        }

        private void AddPrimaryKeyElements(ScanDescriptor rec, IDictionary<String, Object> flat)
        {
            flat.Add(PropertyKeys.KEY_PROJECTID, rec.Project.ProjectId);
            flat.Add(PropertyKeys.KEY_PROJECTNAME, rec.Project.ProjectName);
            flat.Add(PropertyKeys.KEY_TEAMNAME, rec.Project.TeamName);
            if (!String.IsNullOrEmpty(InstanceId))
                flat.Add(PropertyKeys.KEY_INSTANCEID, InstanceId);
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
    }
}
