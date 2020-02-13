using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytics.TransformLogic
{
    /// <summary>
    /// A data object used to describe a scan.
    /// </summary>
    public class ScanDescriptor
    {
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
        public String ScanProduct { get; set; }
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

    }
}
