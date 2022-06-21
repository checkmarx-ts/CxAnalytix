using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Impls;
using System;
using System.Composition;

namespace CxAnalytix.Configuration.ContractImpls
{
    [Export(typeof(ICxAnalytixService))]
    internal sealed class CxAnalytixService : ICxAnalytixService
    {
        private static readonly String SECTION_NAME = "CxAnalyticsService";
        public bool EnablePseudoTransactions => Config.GetSection(SECTION_NAME).EnablePseudoTransactions;

        public string InstanceIdentifier => Config.GetSection(SECTION_NAME).InstanceIdentifier;

        public int ConcurrentThreads => Config.GetSection(SECTION_NAME).ConcurrentThreads;

        public string StateDataStoragePath => Config.GetSection(SECTION_NAME).StateDataStoragePath;

        public string OutputFactoryClassPath => Config.GetSection(SECTION_NAME).OutputFactoryClassPath;

        public string OutputAssembly => Config.GetSection(SECTION_NAME).OutputAssembly;

        public string OutputClass => Config.GetSection(SECTION_NAME).OutputClass;

        public string SASTScanSummaryRecordName => Config.GetSection(SECTION_NAME).SASTScanSummaryRecordName;

        public string SASTScanDetailRecordName => Config.GetSection(SECTION_NAME).SASTScanDetailRecordName;

        public string SCAScanSummaryRecordName => Config.GetSection(SECTION_NAME).SCAScanSummaryRecordName;

        public string SCAScanDetailRecordName => Config.GetSection(SECTION_NAME).SCAScanDetailRecordName;

        public string ProjectInfoRecordName => Config.GetSection(SECTION_NAME).ProjectInfoRecordName;

        public string PolicyViolationsRecordName => Config.GetSection(SECTION_NAME).PolicyViolationsRecordName;

        public int ProcessPeriodMinutes => Config.GetSection(SECTION_NAME).ProcessPeriodMinutes;
    }
}
