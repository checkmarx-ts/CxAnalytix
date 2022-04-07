using CxRestClient.MNO.dto;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace CxAnalytix.TransformLogic.Data
{
    /// <summary>
    /// A data object used to describe a scan.
    /// </summary>
    public class ScanDescriptor : IComparable<ScanDescriptor>
    {

        public enum ScanProductType
		{
            SAST,
            SCA
		}

        internal ScanDescriptor()
        {
            SeverityCounts = new Dictionary<string, long>();
        }

        /// <summary>
        /// The descriptor for the project that owns the scan.
        /// </summary>
        public ProjectDescriptor Project {get; set;}
        /// <summary>
        /// The type of scan that was performed.
        /// </summary>
        public String ScanType { get; set; }
        /// <summary>
        /// The product that performed the scan.
        /// </summary>
        public ScanProductType ScanProduct { get; set; }
        /// <summary>
        /// The scan identifier according to the product.
        /// </summary>
        public String ScanId { get; set; }
        /// <summary>
        /// The timestamp of when the scan finished.
        /// </summary>
        public DateTime FinishedStamp { get; set; }
        /// <summary>
        /// The name of the preset used for this scan, which may differ from
        /// the preset that is currently configured for the project.
        /// </summary>
        public String Preset { get; set; }
        /// <summary>
        /// Stores the counts for each severity.
        /// </summary>
        public Dictionary<String, long> SeverityCounts { get; private set; }

        public String Initiator { get; set; }
        public String DeepLink { get; set; }
        public String ScanTime { get; set; }
        public DateTime ReportCreateTime { get; set; }
        public String Comments { get; set; }
        public String SourceOrigin { get; set; }

        public String Engine { get; set; }

        /// <summary>
        /// Increases the count of a severity with a given name.
        /// </summary>
        /// <param name="sevName"></param>
        public void IncrementSeverity (String sevName)
        {
            if (SeverityCounts.ContainsKey(sevName))
            {
                SeverityCounts[sevName] += 1;
            }
            else
                SeverityCounts.Add(sevName, 1);
        }

        public int PoliciesViolated { get; private set; }
        public int RulesViolated { get; private set; }
        public int Violations { get; private set; }

        private HashSet<int> _policyViolations = new HashSet<int>();
        private HashSet<int> _ruleViolations = new HashSet<int>();

        public bool HasPoliciesApplied { get; private set; }

        public void IncrementPolicyViolations (IEnumerable<ViolatedRuleDescriptor> rules)
        {
            if (rules == null)
                return;
            else
                HasPoliciesApplied = true;

            foreach (var rule in rules)
            {
                Violations++;

                if (!_policyViolations.Contains (rule.PolicyId))
                {
                    PoliciesViolated++;
                    _policyViolations.Add(rule.PolicyId);
                }

                if (!_ruleViolations.Contains(rule.RuleId))
                {
                    RulesViolated++;
                    _ruleViolations.Add(rule.RuleId);
                }
            }

        }

		public int CompareTo([AllowNull] ScanDescriptor other)
		{
            if (other == null)
                return 1;

            return FinishedStamp.CompareTo(other.FinishedStamp);
        }
	}
}
