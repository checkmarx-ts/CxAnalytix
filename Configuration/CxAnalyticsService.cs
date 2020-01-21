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


    }
}
