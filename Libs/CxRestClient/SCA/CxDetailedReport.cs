using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.SCA
{
    public class CxDetailedReport
    {

        private static ILog _log = LogManager.GetLogger(typeof(CxDetailedReport));

        private static String URL_SUFFIX = "risk-management/risk-reports/{0}/export?format=json";

        private CxDetailedReport()
        { }



        [JsonArray]
        public class VulnerabilityIndex : AggregatedCollection<Vulnerability>
        {
            private Dictionary<String, Vulnerability> _index = new();

            public Vulnerability Lookup(String id) => _index[id];

            public override void Add(Vulnerability item)
            {
                base.Add(item);
                _index[item.Id] = item;
            }
        }

        [JsonArray]
        public class LicenseIndex : AggregatedCollection<License>
        {
            private Dictionary<String, License> _index = new();

            public License Lookup(String id) => _index[id];

            public override void Add(License item)
            {
                base.Add(item);
                _index[item.Name] = item;
            }
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class DetailedRiskReport
        {
            [JsonProperty(PropertyName = "RiskReportSummary")]
            public RiskReportSummary Summary { get; internal set; }
            [JsonProperty(PropertyName = "Packages")]
            public List<Package> Packages { get; internal set; }

            [JsonProperty(PropertyName = "Vulnerabilities")]
            public VulnerabilityIndex Vulnerabilities { get; internal set; }
            [JsonProperty(PropertyName = "Licenses")]
            public LicenseIndex Licenses { get; internal set; }
            [JsonProperty(PropertyName = "Policies")]
            public List<Policy> Policies { get; internal set; }


            public override string ToString() =>
                $"{Summary}";

        }

        [JsonObject(MemberSerialization.OptIn)]
        public class RiskReportSummary
        {
            [JsonProperty(PropertyName = "RiskReportId")]
            public String RiskReportId { get; internal set; }
            [JsonProperty(PropertyName = "ProjectId")]
            public String ProjectId { get; internal set; }
            [JsonProperty(PropertyName = "ProjectName")]
            public String ProjectName { get; internal set; }

            [JsonProperty(PropertyName = "HighVulnerabilityCount")]
            public int HighVulnerabilityCount { get; internal set; }
            [JsonProperty(PropertyName = "MediumVulnerabilityCount")]
            public int MediumVulnerabilityCount { get; internal set; }
            [JsonProperty(PropertyName = "LowVulnerabilityCount")]
            public int LowVulnerabilityCount { get; internal set; }

            [JsonProperty(PropertyName = "TotalPackages")]
            public int TotalPackages { get; internal set; }

            [JsonProperty(PropertyName = "DirectPackages")]
            public int DirectPackages { get; internal set; }

            [JsonProperty(PropertyName = "CreatedOn")]
            internal String _reportDate { get; set; }
            public DateTime ReportDate => JsonUtils.NormalizeDateParse(_reportDate);

            [JsonProperty(PropertyName = "RiskScore")]
            public Double RiskScore { get; internal set; }
            [JsonProperty(PropertyName = "TotalOutdatedPackages")]
            public int TotalOutdatedPackages { get; internal set; }

            [JsonProperty(PropertyName = "VulnerablePackages")]
            public int VulnerablePackages { get; internal set; }
            [JsonProperty(PropertyName = "TotalPackagesWithLegalRisk")]
            public int TotalPackagesWithLegalRisk { get; internal set; }

            [JsonProperty(PropertyName = "HighVulnerablePackages")]
            public int HighVulnerablePackages { get; internal set; }

            [JsonProperty(PropertyName = "MediumVulnerablePackages")]
            public int MediumVulnerablePackages { get; internal set; }
            [JsonProperty(PropertyName = "LowVulnerablePackages")]
            public int LowVulnerablePackages { get; internal set; }

            [JsonProperty(PropertyName = "LicensesLegalRisk")]
            public LicensesLegalRisk LicensesLegalRisk { get; internal set;}

            [JsonProperty(PropertyName = "ScanOrigin")]
            public String ScanOrigin { get; internal set; }

            [JsonProperty(PropertyName = "ExploitablePathsFound")]
            public int ExploitablePathsFound { get; internal set; }


            [JsonProperty(PropertyName = "BuildBreakerPolicies")]
            public int BuildBreakerPolicies { get; internal set; }

            [JsonProperty(PropertyName = "ProjectPolicies")]
            public List<String> ProjectPolicies { get; internal set; }

            [JsonProperty(PropertyName = "ViolatingPoliciesCount")]
            public int ViolatingPoliciesCount { get; internal set; }

            public override string ToString() =>
                $"{RiskReportId}:{ProjectName}: Vulns [High: {HighVulnerabilityCount} Medium: {MediumVulnerabilityCount} Low: {LowVulnerabilityCount}]";

        }


        [JsonObject(MemberSerialization.OptIn)]
        public class LicensesLegalRisk
        {
            [JsonProperty(PropertyName = "High")]
            public int High { get; internal set; }

            [JsonProperty(PropertyName = "Medium")]
            public int Medium { get; internal set; }

            [JsonProperty(PropertyName = "Low")]
            public int Low { get; internal set; }

            [JsonProperty(PropertyName = "Unknown")]
            public int Unknown { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class DtoCommon
        {
            [JsonProperty(PropertyName = "Id")]
            public String Id { get; internal set; }

            [JsonProperty(PropertyName = "Severity")]
            public String Severity { get; internal set; }
            [JsonProperty(PropertyName = "IsViolatingPolicy")]
            public Boolean IsViolatingPolicy { get; internal set; }


        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Package : DtoCommon
        {
            [JsonProperty(PropertyName = "Name")]
            public String Name { get; internal set; }
            [JsonProperty(PropertyName = "Version")]
            public String Version { get; internal set; }
            [JsonProperty(PropertyName = "Licenses")]
            public List<String> Licenses { get; internal set; }

            [JsonProperty(PropertyName = "MatchType")]
            public String MatchType { get; internal set; }
            [JsonProperty(PropertyName = "HighVulnerabilityCount")]
            public int High { get; internal set; }

            [JsonProperty(PropertyName = "MediumVulnerabilityCount")]
            public int Medium { get; internal set; }

            [JsonProperty(PropertyName = "LowVulnerabilityCount")]
            public int Low { get; internal set; }

            [JsonProperty(PropertyName = "NumberOfVersionsSinceLastUpdate")]
            public int NumberOfVersionsSinceLastUpdate { get; internal set; }

            [JsonProperty(PropertyName = "NewestVersionReleaseDate")]
            internal String _NewestVersionReleaseDate { get; set; }
            public DateTime NewestVersionReleaseDate => JsonUtils.NormalizeDateParse(_NewestVersionReleaseDate);

            [JsonProperty(PropertyName = "NewestVersion")]
            public String NewestVersion { get; internal set; }

            [JsonProperty(PropertyName = "Outdated")]
            public Boolean Outdated { get; internal set; }

            [JsonProperty(PropertyName = "ReleaseDate")]
            internal String _ReleaseDate { get; set; }
            public DateTime ReleaseDate => JsonUtils.NormalizeDateParse(_ReleaseDate);
            [JsonProperty(PropertyName = "RiskScore")]
            public Double RiskScore { get; internal set; }

            [JsonProperty(PropertyName = "Locations")]
            public List<String> Locations { get; internal set; }
            [JsonProperty(PropertyName = "PackageRepository")]
            public String PackageRepository { get; internal set; }

            [JsonProperty(PropertyName = "IsMalicious")]
            public Boolean IsMalicious { get; internal set; }
            [JsonProperty(PropertyName = "IsDirectDependency")]
            public Boolean IsDirectDependency { get; internal set; }
            [JsonProperty(PropertyName = "IsTestDependency")]
            public Boolean IsTestDependency { get; internal set; }
            [JsonProperty(PropertyName = "UsageType")]
            public String UsageType { get; internal set; }
            [JsonProperty(PropertyName = "VulnerabilityCount")]
            public int VulnerabilityCount { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Vulnerability : DtoCommon
        {
            [JsonProperty(PropertyName = "CveName")]
            public String CveName { get; internal set; }
            [JsonProperty(PropertyName = "Score")]
            public Double Score { get; internal set; }

            [JsonProperty(PropertyName = "PublishDate")]
            internal String _PublishDate { get; set; }
            public DateTime PublishDate => JsonUtils.NormalizeDateParse(_PublishDate);
            [JsonProperty(PropertyName = "References")]
            public List<String> References { get; internal set; }
            [JsonProperty(PropertyName = "Description")]
            public String Description { get; internal set; }

            [JsonProperty(PropertyName = "Cvss")]
            public Cvss Cvss { get; internal set; }
            [JsonProperty(PropertyName = "PackageId")]
            public String PackageId { get; internal set; }
            [JsonProperty(PropertyName = "FixResolutionText")]
            public String FixResolutionText { get; internal set; }
            [JsonProperty(PropertyName = "IsIgnored")]
            public Boolean IsIgnored { get; internal set; }
            [JsonProperty(PropertyName = "ExploitableMethods")]
            public List<String> ExploitableMethods { get; internal set; }
            [JsonProperty(PropertyName = "Cwe")]
            public String Cwe { get; internal set; }

            [JsonProperty(PropertyName = "IsNewInRiskReport")]
            public Boolean IsNewInRiskReport { get; internal set; }

            [JsonProperty(PropertyName = "Type")]
            public String Type { get; internal set; }


        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Cvss
        {
            [JsonProperty(PropertyName = "Score")]
            public Double Score { get; internal set; }
            [JsonProperty(PropertyName = "Severity")]
            public String Severity { get; internal set; }
            [JsonProperty(PropertyName = "AttackVector")]
            public String AttackVector { get; internal set; }
            [JsonProperty(PropertyName = "AttackComplexity")]
            public String AttackComplexity { get; internal set; }
            [JsonProperty(PropertyName = "Confidentiality")]
            public String Confidentiality { get; internal set; }
            [JsonProperty(PropertyName = "Availability")]
            public String Availability { get; internal set; }

            // TODO: ExploitCodeMaturity, RemediationLevel, ReportConfidence, ConfidentialityRequirement, IntegrityRequirement, AvailabilityRequirement
            // types unknown

            [JsonProperty(PropertyName = "Version")]
            public String Version { get; internal set; }
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class License
        {
            [JsonProperty(PropertyName = "ReferenceType")]
            public String ReferenceType { get; internal set; }
            [JsonProperty(PropertyName = "Reference")]
            public String Reference { get; internal set; }
            [JsonProperty(PropertyName = "RoyaltyFree")]
            public String RoyaltyFree { get; internal set; }
            [JsonProperty(PropertyName = "CopyrightRiskScore")]
            public int CopyrightRiskScore { get; internal set; }
            [JsonProperty(PropertyName = "RiskLevel")]
            public String RiskLevel { get; internal set; }
            [JsonProperty(PropertyName = "Linking")]
            public String Linking { get; internal set; }
            [JsonProperty(PropertyName = "CopyLeft")]
            public String CopyLeft { get; internal set; }
            [JsonProperty(PropertyName = "PatentRiskScore")]
            public int PatentRiskScore { get; internal set; }
            [JsonProperty(PropertyName = "Name")]
            public String Name { get; internal set; }
            [JsonProperty(PropertyName = "Url")]
            public String Url { get; internal set; }
            [JsonProperty(PropertyName = "PackageUsageCount")]
            public int PackageUsageCount { get; internal set; }
            [JsonProperty(PropertyName = "IsViolatingPolicy")]
            public Boolean IsViolatingPolicy { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Policy
        {
            [JsonProperty(PropertyName = "Rules")]
            public List<PolicyRule> Rules { get; internal set; }
            [JsonProperty(PropertyName = "Description")]
            public String Description { get; internal set; }
            [JsonProperty(PropertyName = "PolicyName")]
            public String PolicyName { get; internal set; }
            [JsonProperty(PropertyName = "BreakBuild")]
            public Boolean BreakBuild { get; internal set; }
            [JsonProperty(PropertyName = "IsViolating")]
            public Boolean IsViolating { get; internal set; }
            [JsonProperty(PropertyName = "IsGlobal")]
            public Boolean IsGlobal { get; internal set; }
            [JsonProperty(PropertyName = "IsPredefined")]
            public Boolean IsPredefined { get; internal set; }

        }

        [JsonObject(MemberSerialization.OptIn)]
        public class PolicyRule
        {
            [JsonProperty(PropertyName = "Name")]
            public String Name { get; internal set; }
            [JsonProperty(PropertyName = "IsViolated")]
            public Boolean IsViolated { get; internal set; }

            [JsonProperty(PropertyName = "violatingConditionGroups")]
            public List<PolicyRuleConditionGroups> ViolatingConditionGroups { get; internal set; }

        }

        [JsonObject(MemberSerialization.OptIn)]
        public class PolicyRuleConditionGroups
        {
            [JsonProperty(PropertyName = "violatingPackages")]
            public List<PoliyRuleViolatingPackages> ViolatingPackages { get; internal set; }
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class PoliyRuleViolatingPackages
        {
            [JsonProperty(PropertyName = "id")]
            public String Id { get; internal set; }
            [JsonProperty(PropertyName = "ViolatingEntities")]
            public List<PolicyRuleViolation> ViolationDetails { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class PolicyRuleViolation
        {
            [JsonProperty(PropertyName = "id")]
            public String Id { get; internal set; }
            [JsonProperty(PropertyName = "conditionCategory")]
            public String Category { get; internal set; }
        }

        public static DetailedRiskReport GetDetailedReport(CxSCARestContext ctx, CancellationToken token, String riskReportId)
        {
            return WebOperation.ExecuteGet<DetailedRiskReport>(ctx.Json.CreateClient,
                (response) =>
                {
                    return JsonUtils.DeserializeFromStream<DetailedRiskReport>(response.Content.ReadAsStreamAsync().Result);
                },
                UrlUtils.MakeUrl(ctx.ApiUrl, String.Format(URL_SUFFIX, riskReportId)), ctx, token);

        }

    }
}
