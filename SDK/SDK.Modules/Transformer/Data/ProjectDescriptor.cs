using Newtonsoft.Json;
using System;
using static SDK.Modules.Transformer.Data.ScanDescriptor;

namespace SDK.Modules.Transformer.Data
{
    /// <summary>
    /// A data object used to describe a project.
    /// </summary>
    public class ProjectDescriptor
    {
        public ProjectDescriptor()
        {
            ScanCountByProduct = new Dictionary<ScanProductType, int>();
            LatestScanDateByProduct = new Dictionary<ScanProductType, DateTime>();
            ProjectId = String.Empty;
        }

        internal ProjectDescriptor (ProjectDescriptor src)
        {
            PresetId = src.PresetId;
            PresetName = src.PresetName;
            ProjectId = src.ProjectId;
            ProjectName = src.ProjectName;
            TeamId = src.TeamId;
            TeamName = src.TeamName;
            Policies = src.Policies;
            ScanCountByProduct = src.ScanCountByProduct;
            LatestScanDateByProduct = src.LatestScanDateByProduct;
            CustomFields = src.CustomFields;
            IsBranched = src.IsBranched;
            BranchedAtScanId = src.BranchedAtScanId;
            BranchParentProject = src.BranchParentProject;
        }

        [JsonIgnore]
        public bool IsBranched { get; set; }

        [JsonIgnore]
        public String BranchParentProject { get; set; }

        [JsonIgnore]
        public String BranchedAtScanId { get; set; }

        /// <summary>
        /// Custom fields assigned to the project.
        /// </summary>
        [JsonIgnore]
        public IDictionary<String, String>? CustomFields { get; set; }
        /// <summary>
        /// The project id as it is stored in the SAST system.
        /// </summary>
        public String ProjectId { get; set; }
        /// <summary>
        /// The name of the project.
        /// </summary>
        public String? ProjectName { get; set; }
        /// <summary>
        /// The GUID or integer that identifies the team that owns the project.
        /// </summary>
        public String? TeamId { get; set; }
        /// <summary>
        /// A human-readable name for the team.
        /// </summary>
        public String? TeamName { get; set; }
        /// <summary>
        /// A numeric identifier of the preset configured for the project.
        /// </summary>
        public String? PresetId { get; set; }
        /// <summary>
        /// A human-eadable name for the preset.
        /// </summary>
        [JsonIgnore]
        public String? PresetName { get; set; }
        /// <summary>
        /// The M&O policies assigned to the project, if any.
        /// </summary>
        [JsonIgnore]
        public String? Policies { get; set; }
        /// <summary>
        /// A dictionary showing the number of scans for the project by product.
        /// SAST and SCA scans (and maybe others in the future) are not restricted
        /// to a 1:1 match for each scan.
        /// </summary>
        [JsonIgnore]
        public Dictionary<ScanProductType, int> ScanCountByProduct { get; internal set; }
        /// <summary>
        /// The most recent scan date for the project by product.
        /// </summary>
        [JsonIgnore]
        public Dictionary<ScanProductType, DateTime> LatestScanDateByProduct { get; internal set; }

        /// <summary>
        /// Increments the number of scans for the product name passed in 
        /// the <paramref name="scanProduct"/> parameter.
        /// </summary>
        /// <param name="scanProduct">The name of the product.</param>
        public void IncrementScanCount(ScanProductType scanProduct)
        {
            if (!ScanCountByProduct.ContainsKey(scanProduct))
            {
                ScanCountByProduct.Add(scanProduct, 0);
            }

            ScanCountByProduct[scanProduct] = ScanCountByProduct[scanProduct] + 1;
        }


        /// <summary>
        /// Updates the latest scan date for the product name passed in the
        /// <paramref name="scanProduct"/> parameter.
        /// </summary>
        /// <param name="scanTime">The time of a scan.</param>
        /// <param name="scanProduct">The product that performed the scan.</param>
        public void UpdateLatestScanDate(ScanProductType scanProduct, DateTime scanTime)
        {
            if (!LatestScanDateByProduct.ContainsKey(scanProduct))
            {
                LatestScanDateByProduct.Add(scanProduct, DateTime.MinValue);
            }

            if (LatestScanDateByProduct[scanProduct].CompareTo(scanTime) < 0)
                LatestScanDateByProduct[scanProduct] = scanTime;
        }

    }
}
