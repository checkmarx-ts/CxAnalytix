using System;
using System.Diagnostics.CodeAnalysis;

namespace SDK.Modules.Transformer.Data
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

        public ScanDescriptor()
        {
            SeverityCounts = new();
            ViolatedPolicies = new();
        }

        /// <summary>
        /// The descriptor for the project that owns the scan.
        /// </summary>
        public ProjectDescriptor? Project {get; set;}
        /// <summary>
        /// The type of scan that was performed.
        /// </summary>
        public String? ScanType { get; set; }
        /// <summary>
        /// The product that performed the scan.
        /// </summary>
        public ScanProductType? ScanProduct { get; set; }
        /// <summary>
        /// The scan identifier according to the product.
        /// </summary>
        public String? ScanId { get; set; }
        /// <summary>
        /// The timestamp of when the scan finished.
        /// </summary>
        public DateTime FinishedStamp { get; set; }
        /// <summary>
        /// The name of the preset used for this scan, which may differ from
        /// the preset that is currently configured for the project.
        /// </summary>
        public String? Preset { get; set; }
        /// <summary>
        /// Stores the counts for each severity.
        /// </summary>
        public Dictionary<String, long> SeverityCounts { get; private set; }

        public String? Initiator { get; set; }
        public String? DeepLink { get; set; }
        public String? ScanTime { get; set; }
        public DateTime ReportCreateTime { get; set; }
        public String? Comments { get; set; }
        public String? SourceOrigin { get; set; }

        public String? Engine { get; set; }

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

        public HashSet<String> ViolatedPolicies { get; private set; }
        private HashSet<String> _ruleViolations = new HashSet<String>();

        public bool HasPoliciesApplied { get; private set; }

        public void IncrementPolicyViolation (String policyId, String ruleId)
        {
            if (policyId == null || ruleId == null)
                return;
            else
                HasPoliciesApplied = true;

            Violations++;

            if (!ViolatedPolicies.Contains (policyId))
            {
                PoliciesViolated++;
                ViolatedPolicies.Add(policyId);
            }

            if (!_ruleViolations.Contains(ruleId))
            {
                RulesViolated++;
                _ruleViolations.Add(ruleId);
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
