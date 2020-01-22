using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytics.Configuration
{
    public sealed class CxAnalyticsService : ConfigurationSection
    {
        internal CxAnalyticsService ()
        {

        }

        public static readonly String SECTION_NAME = "CxAnalyticsService";

        [ConfigurationProperty("ConcurrentThreads", IsRequired = true)]
        public int ConcurrentThreads
        {
            get => (int)this["ConcurrentThreads"];
            set { this["ConcurrentThreads"] = value; }
        }

        [ConfigurationProperty("StateDataFile", IsRequired = true)]
        public String StateDataFile
        {
            get => (String)this["StateDataFile"];
            set { this["StateDataFile"] = value; }
        }

        [ConfigurationProperty("OutputAssembly", IsRequired = true)]
        public String OutputAssembly
        {
            get => (String)this["OutputAssembly"];
            set { this["OutputAssembly"] = value; }
        }

        [ConfigurationProperty("SASTScanSummaryRecordName", IsRequired = true)]
        public String SASTScanSummaryRecordName
        {
            get => (String)this["SASTScanSummaryRecordName"];
            set { this["SASTScanSummaryRecordName"] = value; }
        }

        [ConfigurationProperty("SASTScanDetailRecordName", IsRequired = true)]
        public String SASTScanDetailRecordName
        {
            get => (String)this["SASTScanDetailRecordName"];
            set { this["SASTScanDetailRecordName"] = value; }
        }

        [ConfigurationProperty("SCAScanSummaryRecordName", IsRequired = true)]
        public String SCAScanSummaryRecordName
        {
            get => (String)this["SCAScanSummaryRecordName"];
            set { this["SCAScanSummaryRecordName"] = value; }
        }

        [ConfigurationProperty("SCAScanDetailRecordName", IsRequired = true)]
        public String SCAScanDetailRecordName
        {
            get => (String)this["SCAScanDetailRecordName"];
            set { this["SCAScanDetailRecordName"] = value; }
        }

        [ConfigurationProperty("ProjectInfoRecordName", IsRequired = true)]
        public String ProjectInfoRecordName
        {
            get => (String)this["ProjectInfoRecordName"];
            set { this["ProjectInfoRecordName"] = value; }
        }

        [ConfigurationProperty("PolicyViolationsRecordName", IsRequired = true)]
        public String PolicyViolationsRecordName
        {
            get => (String)this["PolicyViolationsRecordName"];
            set { this["PolicyViolationsRecordName"] = value; }
        }

        [ConfigurationProperty("ProcessPeriodMinutes", IsRequired = true)]
        public int ProcessPeriodMinutes
        {
            get => (int)this["ProcessPeriodMinutes"];
            set { this["ProcessPeriodMinutes"] = value; }
        }
        

    }
}
