using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytics.TransformLogic
{
    /// <summary>
    /// A class that is used to provide the names of supported record types to the transformation logic.
    /// </summary>
    public class RecordNames
    {
        public String SASTScanSummary { get; set; }
        public String SASTScanDetail { get; set; }
        public String SCAScanSummary { get; set; }
        public String SCAScanDetail { get; set; }
        public String ProjectInfo { get; set; }
        public String PolicyViolations { get; set; }

    }
}
