using CxAnalytix.Configuration.Utils;
using System;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{

    public sealed class CxAnalytixService : EnvAwareConfigurationSection
    {
        public CxAnalytixService() {}


        [ConfigurationProperty("EnablePseudoTransactions", IsRequired = false, DefaultValue = false)]
        public bool EnablePseudoTransactions
        {
            get => (bool)this["EnablePseudoTransactions"];
            set { this["EnablePseudoTransactions"] = value; }
        }


        [ConfigurationProperty("InstanceId", IsRequired = false)]
        public String InstanceIdentifier
        {
            get => (String)this["InstanceId"];
            set { this["InstanceId"] = value; }
        }


        [ConfigurationProperty("ConcurrentThreads", IsRequired = true)]
        public int ConcurrentThreads
        {
            get => (int)this["ConcurrentThreads"];
            set { this["ConcurrentThreads"] = value; }
        }

        [ConfigurationProperty("StateDataStoragePath", IsRequired = true)]
        public String StateDataStoragePath
        {
            get => (String)this["StateDataStoragePath"];
            set { this["StateDataStoragePath"] = value; }
        }

        [ConfigurationProperty("OutputModuleName", IsRequired = true)]
        public String OutputModuleName
        {
            get => (String)this["OutputModuleName"];
            set { this["OutputModuleName"] = value; }
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
