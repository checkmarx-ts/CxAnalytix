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

        [ConfigurationProperty("OutputFactoryClassPath", IsRequired = true)]
        public String OutputFactoryClassPath
        {
            get => (String)this["OutputFactoryClassPath"];
            set { this["OutputFactoryClassPath"] = value; }
        }

        public String OutputAssembly
        {
            get
            {
                String [] components = OutputFactoryClassPath.Split(',');

                if (components.Length < 2)
                    throw new Exception(String.Format ("OutputClassPath value [{0}] does not specify the assembly.", 
                        OutputFactoryClassPath) );

                return components[1];

            }
        }

        public String OutputClass
        {
            get => OutputFactoryClassPath.Split(',')[0];
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
