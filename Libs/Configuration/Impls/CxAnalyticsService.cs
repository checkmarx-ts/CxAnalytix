using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Utils;
using System;
using System.Composition;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{

    [Export(typeof(ICxAnalytixService))]
    public sealed class CxAnalyticsService : EnvAwareConfigurationSection, ICxAnalytixService
    {
        public CxAnalyticsService() {}


        [ImportingConstructor]
        public CxAnalyticsService(IConfigSectionResolver resolver) : base(resolver) {}


        [ConfigurationProperty("EnablePseudoTransactions", IsRequired = false, DefaultValue = false)]
        public bool EnablePseudoTransactions
        {
            get => (bool)Instance<CxAnalyticsService>()["EnablePseudoTransactions"];
            set { Instance<CxAnalyticsService>()["EnablePseudoTransactions"] = value; }
        }


        [ConfigurationProperty("InstanceId", IsRequired = false)]
        public String InstanceIdentifier
        {
            get => (String)Instance<CxAnalyticsService>()["InstanceId"];
            set { Instance<CxAnalyticsService>()["InstanceId"] = value; }
        }


        [ConfigurationProperty("ConcurrentThreads", IsRequired = true)]
        public int ConcurrentThreads
        {
            get => (int)Instance<CxAnalyticsService>()["ConcurrentThreads"];
            set { Instance<CxAnalyticsService>()["ConcurrentThreads"] = value; }
        }

        [ConfigurationProperty("StateDataStoragePath", IsRequired = true)]
        public String StateDataStoragePath
        {
            get => (String)Instance<CxAnalyticsService>()["StateDataStoragePath"];
            set { Instance<CxAnalyticsService>()["StateDataStoragePath"] = value; }
        }

        [ConfigurationProperty("OutputModuleName", IsRequired = true)]
        public String OutputModuleName
        {
            get => (String)Instance<CxAnalyticsService>()["OutputModuleName"];
            set { Instance<CxAnalyticsService>()["OutputModuleName"] = value; }
        }

        [ConfigurationProperty("SASTScanSummaryRecordName", IsRequired = true)]
        public String SASTScanSummaryRecordName
        {
            get => (String)Instance<CxAnalyticsService>()["SASTScanSummaryRecordName"];
            set { Instance<CxAnalyticsService>()["SASTScanSummaryRecordName"] = value; }
        }

        [ConfigurationProperty("SASTScanDetailRecordName", IsRequired = true)]
        public String SASTScanDetailRecordName
        {
            get => (String)Instance<CxAnalyticsService>()["SASTScanDetailRecordName"];
            set { Instance<CxAnalyticsService>()["SASTScanDetailRecordName"] = value; }
        }

        [ConfigurationProperty("SCAScanSummaryRecordName", IsRequired = true)]
        public String SCAScanSummaryRecordName
        {
            get => (String)Instance<CxAnalyticsService>()["SCAScanSummaryRecordName"];
            set { Instance<CxAnalyticsService>()["SCAScanSummaryRecordName"] = value; }
        }

        [ConfigurationProperty("SCAScanDetailRecordName", IsRequired = true)]
        public String SCAScanDetailRecordName
        {
            get => (String)Instance<CxAnalyticsService>()["SCAScanDetailRecordName"];
            set { Instance<CxAnalyticsService>()["SCAScanDetailRecordName"] = value; }
        }

        [ConfigurationProperty("ProjectInfoRecordName", IsRequired = true)]
        public String ProjectInfoRecordName
        {
            get => (String)Instance<CxAnalyticsService>()["ProjectInfoRecordName"];
            set { Instance<CxAnalyticsService>()["ProjectInfoRecordName"] = value; }
        }

        [ConfigurationProperty("PolicyViolationsRecordName", IsRequired = true)]
        public String PolicyViolationsRecordName
        {
            get => (String)Instance<CxAnalyticsService>()["PolicyViolationsRecordName"];
            set { Instance<CxAnalyticsService>()["PolicyViolationsRecordName"] = value; }
        }

        [ConfigurationProperty("ProcessPeriodMinutes", IsRequired = true)]
        public int ProcessPeriodMinutes
        {
            get => (int)Instance<CxAnalyticsService>()["ProcessPeriodMinutes"];
            set { Instance<CxAnalyticsService>()["ProcessPeriodMinutes"] = value; }
        }

    }
}
