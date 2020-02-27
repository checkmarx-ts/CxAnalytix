using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytics.TransformLogic
{
    /// <summary>
    /// A data object used to describe a project.
    /// </summary>
    public class ProjectDescriptor
    {
        internal ProjectDescriptor()
        {
            ScanCountByProduct = new Dictionary<string, int>();
            LatestScanDateByProduct = new Dictionary<string, DateTime>();
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
        }

        /// <summary>
        /// The project id as it is stored in the SAST system.
        /// </summary>
        public int ProjectId { get; set; }
        /// <summary>
        /// The name of the project.
        /// </summary>
        public String ProjectName { get; set; }
        /// <summary>
        /// The GUID that identifies the team that owns the project.
        /// </summary>
        public Guid TeamId { get; set; }
        /// <summary>
        /// A human-readable name for the team.
        /// </summary>
        public String TeamName { get; set; }
        /// <summary>
        /// A numeric identifier of the preset configured for the project.
        /// </summary>
        public int PresetId { get; set; }
        /// <summary>
        /// A human-eadable name for the preset.
        /// </summary>
        [JsonIgnore]
        public String PresetName { get; set; }
        /// <summary>
        /// The M&O policies assigned to the project, if any.
        /// </summary>
        [JsonIgnore]
        public String Policies { get; set; }
        /// <summary>
        /// A dictionary showing the number of scans for the project by product.
        /// SAST and SCA scans (and maybe others in the future) are not restricted
        /// to a 1:1 match for each scan.
        /// </summary>
        [JsonIgnore]
        public Dictionary<String, int> ScanCountByProduct { get; internal set; }
        /// <summary>
        /// The most recent scan date for the project by product.
        /// </summary>
        [JsonIgnore]
        public Dictionary<String, DateTime> LatestScanDateByProduct { get; internal set; }

        /// <summary>
        /// Increments the number of scans for the product name passed in 
        /// the <paramref name="scanProduct"/> parameter.
        /// </summary>
        /// <param name="scanProduct">The name of the product.</param>
        public void IncrementScanCount(String scanProduct)
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
        public void UpdateLatestScanDate(String scanProduct, DateTime scanTime)
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
