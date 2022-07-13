using CxAnalytix.Configuration.Impls;
using CxAnalytix.XForm.Common;
using CxAnalytix.XForm.ScaTransformer.Config;
using CxRestClient;
using CxRestClient.SCA;
using log4net;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using SDK.Modules.Transformer.Data;
using System.Collections.Concurrent;
using CxAnalytix.Extensions;
using static SDK.Modules.Transformer.Data.ScanDescriptor;
using OutputBootstrapper;
using CxAnalytix.XForm.ScaTransformer.Policy;
using ScanHeaderIndex_t = System.Collections.Generic.Dictionary<string, CxRestClient.SCA.CxScans.Scan>;
using CxAnalytix.Interfaces.Outputs;

namespace CxAnalytix.XForm.ScaTransformer
{
	public class Transformer : BaseTransformer
	{
        private static readonly ILog _log = LogManager.GetLogger(typeof(Transformer));
        private static readonly String STATE_STORAGE_FILE = "CxAnalytixExportState_SCA.json";
		private static readonly String MODULE_NAME = "SCA";

        private Task<ScaPolicyIndex> PoliciesTask { get; set; }
        private ScaPolicyIndex Policies => PoliciesTask.Result;

        private ScanHeaderIndex_t ScanHeaderIndex { get; set; }


        public Transformer() : base(MODULE_NAME, typeof(Transformer), STATE_STORAGE_FILE)
		{
            ScanHeaderIndex = new();
		}

		public override string DisplayName => MODULE_NAME;


        private async Task<ScaPolicyIndex> PopulatePolicies(CxSCARestContext ctx)
        {
            return await Task.Run(
                () =>
                {
                    _log.Debug("Retrieving SCA policies.");

                    return new ScaPolicyIndex(CxPolicies.GetPolicies(ctx, ThreadOpts.CancellationToken));
                }, ThreadOpts.CancellationToken);
        }

        public override void DoTransform(CancellationToken token)
		{
			ThreadOpts.CancellationToken = token;

            var apiConCfg = Configuration.Impls.Config.GetConfig<CxScaConnection>();
			var loginCreds = Configuration.Impls.Config.GetConfig<CxMultiTenantCredentials>();

            var restBuilder = new CxSCARestContext.CxSCARestContextBuilder();

            restBuilder
            .WithApiURL(apiConCfg.URL)
            .WithLoginUrl(apiConCfg.LoginURL)
            .WithTenant(loginCreds.Tenant)
            .WithUsername(loginCreds.Username)
            .WithPassword(loginCreds.Password)
            .WithOpTimeout(apiConCfg.TimeoutSeconds)
            .WithSSLValidate(apiConCfg.ValidateCertificates)
            .WithRetryLoop(apiConCfg.RetryLoop);

            var ctx = restBuilder.Build();

            PoliciesTask = PopulatePolicies(ctx);

            _log.Debug("Retrieving SCA projects.");
            var projects = CxProjects.GetProjects(ctx, ThreadOpts.CancellationToken);

            var projectDescriptors = new ConcurrentBag<ProjectDescriptor>();


            _log.Debug("Resolving projects.");


            Parallel.ForEach(projects, ThreadOpts, (p) => {

                // Projects don't need to have a team assignment, unlike in SAST
                if (! ( (p.Teams.Count == 0) ? Filter.Matches(p.ProjectName) : p.Teams.Any((t) => Filter.Matches(t, p.ProjectName))))
                {
                    if (_log.IsDebugEnabled)
                        _log.Debug($"FILTERED: Project: [{p.ProjectName}] with assigned teams [{String.Join(",", p.Teams)}]");
                    return;
                }
                else
                {
                    projectDescriptors.Add(new ProjectDescriptor()
                    {
                        ProjectId = p.ProjectId,
                        ProjectName = p.ProjectName,
                        TeamName = String.Join(";", p.Teams),
                        Policies = String.Join(";", Policies.GetPoliciesForProject(p.ProjectId).Select((policy) => policy.Name))
                    });
                }
            });

            State.ConfirmProjects(projectDescriptors);

            _log.Info($"{State.ProjectCount} projects are targets to check for new scans. Since last crawl: {State.DeletedProjects}"
                + $" projects removed, {State.NewProjects} new projects.");

            _log.Debug("Resolving scans.");

            Parallel.ForEach(State.Projects, ThreadOpts, (p) => {

                var scans = CxScans.GetCompletedScans(ctx, ThreadOpts.CancellationToken, p.ProjectId);

                foreach (var s in scans)
                {
                    // Add to crawl state.
                    if (_log.IsTraceEnabled())
                        _log.Trace($"SCA scan record: {s}");
                    State.AddScan(Convert.ToString(s.ProjectId), s.Origin, ScanProductType.SCA, s.ScanId, s.Updated, null);

                    ScanHeaderIndex.Add(s.ScanId, s);
                }

            });

            if (State.Projects == null)
            {
                _log.Error("Scans to crawl do not appear to be resolved, unable to crawl scan data.");
                return;
            }

            _log.Info($"Crawling {State.ScanCount} scans.");


            Parallel.ForEach<ProjectDescriptor>(State.Projects, ThreadOpts,
            (project) => {

                if (State.GetScanCountForProject(project.ProjectId) <= 0)
                {
                    _log.Info($"Project {project.ProjectId}:{project.TeamName}:{project.ProjectName} has no new scans to process.");
                    return;
                }

                using (var pinfoTrx = Output.StartTransaction())
                {
                    OutputProjectInfoRecords(pinfoTrx, project);

                    if (!ThreadOpts.CancellationToken.IsCancellationRequested)
                        pinfoTrx.Commit();
                }


                var riskStateTask = Task.Run(() => CxRiskState.GetRiskStates(ctx, ThreadOpts.CancellationToken, project.ProjectId),
                    ThreadOpts.CancellationToken);


                foreach (var scan in State.GetScansForProject(project.ProjectId))
                {
                    if (ThreadOpts.CancellationToken.IsCancellationRequested)
                        break;


                    using (var scanTrx = Output.StartTransaction())
                    {
                        _log.Info($"Processing {scan.ScanProduct} scan {scan.ScanId}:{scan.Project.ProjectId}:{ScanHeaderIndex[scan.ScanId].RiskReportId}:{scan.Project.ProjectName}[{scan.FinishedStamp}]");


                        var riskReport = CxDetailedReport.GetDetailedReport(ctx, ThreadOpts.CancellationToken, ScanHeaderIndex[scan.ScanId].RiskReportId);

                        foreach (var policy in riskReport.Policies)
                            if (policy.IsViolating)
                                foreach (var rule in policy.Rules)
                                    if (rule.IsViolated)
                                        scan.IncrementPolicyViolation(policy.PolicyName, rule.Name);

                        OutputScanSummary(scanTrx, scan, riskReport);



                        OutputPolicyViolations(scanTrx, scan, riskReport, riskStateTask.Result);


                        // TODO: Policy violations
                        // TODO: Scan summary
                        // TODO: Scan details

                        if (!ThreadOpts.CancellationToken.IsCancellationRequested && scanTrx.Commit())
                        {
                            State.ScanCompleted(scan);
                            continue;
                        }

                    }
                }
            });

		}

        private void OutputPolicyViolations(IOutputTransaction trx, ScanDescriptor sd, CxDetailedReport.DetailedRiskReport riskReport, CxRiskState.IndexedRiskStates stateIndex)
        {


            var header = new SortedDictionary<String, Object>();
            AddPrimaryKeyElements(sd.Project, header);
            header.Add("ScanProduct", sd.ScanProduct.ToString());
            header.Add("ScanType", sd.ScanType);
            header.Add("ScanId", sd.ScanId);

            foreach(var policy in riskReport.Policies)
            {
                if (!policy.IsViolating)
                    continue;

                var policy_flat = new SortedDictionary<String, Object>(header);
                policy_flat.Add("PolicyName", policy.PolicyName);
                foreach(var rule in policy.Rules)
                {
                    if (!rule.IsViolated)
                        continue;

                    var rule_flat = new SortedDictionary<String, Object>(policy_flat);
                    rule_flat.Add("RuleName", rule.Name);
                    foreach(var cg in rule.ViolatingConditionGroups)
                    {
                        foreach(var vp in cg.ViolatingPackages)
                        {
                            var package_flat = new SortedDictionary<String, Object>(rule_flat);
                            package_flat.Add("ViolationName", vp.Id);
                            foreach(var violation_details in vp.ViolationDetails)
                            {
                                var detail_flat = new SortedDictionary<String, Object>(package_flat);
                                detail_flat.Add("ViolationId", violation_details.Id);
                                detail_flat.Add("RuleType", violation_details.Category);
                                detail_flat.Add("ViolationOccurredDate", sd.FinishedStamp);

                                AddTypeSpecificPolicyViolationDetails(detail_flat, riskReport, violation_details, stateIndex);

                                trx.write(PolicyViolationDetailOut, detail_flat);
                            }
                        }
                    }
                }
            }
        }

        private void AddTypeSpecificPolicyViolationDetails(SortedDictionary<string, object> detail_flat, CxDetailedReport.DetailedRiskReport riskReport,
            CxDetailedReport.PolicyRuleViolation violationDetails, CxRiskState.IndexedRiskStates stateIndex)
        {
            switch(violationDetails.Category)
            {
                case "Vulnerability":
                    var specificVuln = riskReport.Vulnerabilities.Lookup(violationDetails.Id);
                    detail_flat.Add("ViolationRiskScore", specificVuln.Score);
                    detail_flat.Add("ViolationSeverity", specificVuln.Severity);
                    detail_flat.Add("ViolationStatus", specificVuln.IsNewInRiskReport ? "New" : "Recurrent");
                    detail_flat.Add("ViolationState", stateIndex.Lookup(specificVuln.PackageId, specificVuln.Id).State);
                    detail_flat.Add("CveName", specificVuln.CveName);
                    detail_flat.Add("PublishDate", specificVuln.PublishDate);
                    detail_flat.Add("CVSS_Score", specificVuln.Cvss.Score);
                    detail_flat.Add("CVSS_Severity", specificVuln.Cvss.Severity);
                    detail_flat.Add("CVSS_AttackVector", specificVuln.Cvss.AttackVector);
                    detail_flat.Add("CVSS_AttackComplexity", specificVuln.Cvss.AttackComplexity);
                    detail_flat.Add("CVSS_Confidentiality", specificVuln.Cvss.Confidentiality);
                    detail_flat.Add("CVSS_Availability", specificVuln.Cvss.Availability);
                    detail_flat.Add("CVSS_Version", specificVuln.Cvss.Version);
                    detail_flat.Add("Cwe", specificVuln.Cwe);
                    detail_flat.Add("Type", specificVuln.Type);
                    break;

                case "License":
                    var specificLicense = riskReport.Licenses.Lookup(violationDetails.Id);
                    detail_flat.Add("ViolationSeverity", specificLicense.RiskLevel);
                    detail_flat.Add("ReferenceType", specificLicense.ReferenceType);
                    detail_flat.Add("Reference", specificLicense.Reference);
                    detail_flat.Add("RoyaltyFree", specificLicense.RoyaltyFree);
                    detail_flat.Add("CopyrightRiskScore", specificLicense.CopyrightRiskScore);
                    detail_flat.Add("CopyLeft", specificLicense.CopyLeft);
                    detail_flat.Add("Linking", specificLicense.Linking);
                    detail_flat.Add("PatentRiskScore", specificLicense.PatentRiskScore);
                    detail_flat.Add("LicenseUrl", specificLicense.Url);
                    detail_flat.Add("PackageUsageCount", specificLicense.PackageUsageCount);
                    break;

                default:
                    _log.Warn($"Unknown policy violation category [{violationDetails.Category}] found in project [{riskReport.Summary.ProjectName}]");
                    break;
            }
        }

        private void OutputScanSummary(IOutputTransaction trx, ScanDescriptor sd, CxDetailedReport.DetailedRiskReport report)
        {
            var flat = new SortedDictionary<String, Object>();
            AddPrimaryKeyElements(sd.Project, flat);
            flat.Add("ScanId", sd.ScanId);
            flat.Add("ScanStart", ScanHeaderIndex[sd.ScanId].Created);
            flat.Add("ScanFinished", ScanHeaderIndex[sd.ScanId].Updated);

            flat.Add("LegalHigh", report.Summary.LicensesLegalRisk.High);
            flat.Add("LegalMedium", report.Summary.LicensesLegalRisk.Medium);
            flat.Add("LegalLow", report.Summary.LicensesLegalRisk.Low);
            flat.Add("LegalUnknown", report.Summary.LicensesLegalRisk.Unknown);

            // TODO: NEW
            flat.Add("TotalLegalRiskPackages", report.Summary.TotalPackagesWithLegalRisk);

            flat.Add("TotalHighVulnerabilities", report.Summary.HighVulnerabilityCount);
            flat.Add("TotalMediumVulnerabilities", report.Summary.MediumVulnerabilityCount);
            flat.Add("TotalLowVulnerabilities", report.Summary.LowVulnerabilityCount);

            flat.Add("TotalLibraries", report.Summary.TotalPackages);

            // As of July 11, 2022 the counts here are not accurate.  The risk report does not account for
            // cases where all vulnerabilities in a package are marked NE.  The counts here are packages
            // that have at least one reported severity, even if the severity is marked as NE.
            // If SCA fixes the totalling method, this will be correct.
            flat.Add("HighVulnerabilityLibraries", report.Summary.HighVulnerablePackages);
            flat.Add("MediumVulnerabilityLibraries", report.Summary.MediumVulnerablePackages);
            flat.Add("LowVulnerabilityLibraries", report.Summary.LowVulnerablePackages);

            // Due to the counts above not being accurate, this total is not accurate.
            flat.Add("NonVulnerableLibraries", report.Summary.TotalPackages - 
                (report.Summary.HighVulnerablePackages + report.Summary.MediumVulnerablePackages + report.Summary.LowVulnerablePackages) );

            flat.Add("VulnerabilityScore", report.Summary.RiskScore);

            int outdatedVuln = 0;
            int updatedVuln = 0;

            HashSet<String> outDatedPackages = new();
            HashSet<String> currentPackages = new();
            foreach (var pkg in report.Packages)
            {
                if (pkg.Outdated)
                    outDatedPackages.Add(pkg.Id);
                else
                    currentPackages.Add(pkg.Id);
            }

            foreach(var vuln in report.Vulnerabilities)
            {
                if (vuln.IsIgnored)
                    continue;

                if (outDatedPackages.Contains(vuln.PackageId) )
                {
                    outdatedVuln++;
                    continue;
                }

                if (currentPackages.Contains(vuln.PackageId))
                {
                    updatedVuln++;
                    continue;
                }

                // It should be impossible to see this in the log if we assume the detailed report is compiled correctly.
                _log.Warn($"Cannot determine if {vuln.PackageId} is current or outdated in project {sd.Project.ProjectName}.");
            }

            flat.Add("VulnerableAndOutdated", outdatedVuln);
            flat.Add("VulnerableAndUpdated", updatedVuln);

            if (sd.HasPoliciesApplied && sd.PoliciesViolated > 0)
            {
                flat.Add("RulesViolated", sd.RulesViolated);
                flat.Add("PolicyViolations", sd.PoliciesViolated);
                flat.Add("PoliciesViolated", String.Join(";", sd.ViolatedPolicies));
            }

            // TODO: NEW
            flat.Add("TotalDirectDependencies", report.Summary.DirectPackages);
            // TODO: NEW
            flat.Add("ScanOrigin", report.Summary.ScanOrigin);
            // TODO: NEW
            flat.Add("TotalExploitablePaths", report.Summary.ExploitablePathsFound);

            trx.write(ScaScanSummaryOut, flat);
        }


    }
}
