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

    }
}
