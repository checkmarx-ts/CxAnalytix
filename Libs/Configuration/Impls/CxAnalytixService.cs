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

        [RecordNameConfig]
        [ConfigurationProperty("SASTScanSummaryRecordName", IsRequired = false, DefaultValue = null)]
        public String SASTScanSummaryRecordName
        {
            get => (String)this["SASTScanSummaryRecordName"];
            set { this["SASTScanSummaryRecordName"] = value; }
        }

        [ConfigurationProperty("SASTScanDetailRecordName", IsRequired = false, DefaultValue = null)]
        [RecordNameConfig]
        public String SASTScanDetailRecordName
        {
            get => (String)this["SASTScanDetailRecordName"];
            set { this["SASTScanDetailRecordName"] = value; }
        }

        [ConfigurationProperty("SCAScanSummaryRecordName", IsRequired = false, DefaultValue = null)]
        [RecordNameConfig]
        public String SCAScanSummaryRecordName
        {
            get => (String)this["SCAScanSummaryRecordName"];
            set { this["SCAScanSummaryRecordName"] = value; }
        }

        [ConfigurationProperty("SCAScanDetailRecordName", IsRequired = false, DefaultValue = null)]
        [RecordNameConfig]
        public String SCAScanDetailRecordName
        {
            get => (String)this["SCAScanDetailRecordName"];
            set { this["SCAScanDetailRecordName"] = value; }
        }

        [ConfigurationProperty("ProjectInfoRecordName", IsRequired = false, DefaultValue = null)]
        [RecordNameConfig]
        public String ProjectInfoRecordName
        {
            get => (String)this["ProjectInfoRecordName"];
            set { this["ProjectInfoRecordName"] = value; }
        }

        [ConfigurationProperty("PolicyViolationsRecordName", IsRequired = false, DefaultValue = null)]
        [RecordNameConfig]
        public String PolicyViolationsRecordName
        {
            get => (String)this["PolicyViolationsRecordName"];
            set { this["PolicyViolationsRecordName"] = value; }
        }

        [ConfigurationProperty("ScanStatisticsRecordName", IsRequired = false, DefaultValue = null)]
        [RecordNameConfig]
        public String ScanStatisticsRecordName
        {
            get => (String)this["ScanStatisticsRecordName"];
            set { this["ScanStatisticsRecordName"] = value; }
        }

        [ConfigurationProperty("ProcessPeriodMinutes", IsRequired = true)]
        public int ProcessPeriodMinutes
        {
            get => (int)this["ProcessPeriodMinutes"];
            set { this["ProcessPeriodMinutes"] = value; }
        }

        [ConfigurationProperty("EnabledTransformers", IsDefaultCollection = false, IsRequired = true)]
        [ConfigurationCollection(typeof(ConfigElementCollection<EnabledTransformer>), AddItemName = "Transformer")]
        public ConfigElementCollection<EnabledTransformer> Transformers
        {
            get => (ConfigElementCollection <EnabledTransformer>)this["EnabledTransformers"];
            set => this["EnabledTransformers"] = value;
        }

    }
}
