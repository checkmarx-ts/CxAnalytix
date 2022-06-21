﻿using CxAnalytix.Configuration.Utils;
using System;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
    internal sealed class CxAnalyticsService : EnvAwareConfigurationSection
    {


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
