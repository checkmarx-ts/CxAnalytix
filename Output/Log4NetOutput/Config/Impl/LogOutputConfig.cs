using CxAnalytix.Out.Log4NetOutput.Config.Contracts;
using System;
using System.Configuration;

namespace CxAnalytix.Out.Log4NetOutput.Config.Impl
{

    public sealed class LogOutputConfig : ConfigurationSection
    {
        public LogOutputConfig() { }


        [ConfigurationProperty("DataRetentionDays", IsRequired = true)]
        public int DataRetentionDays
        {
            get => (int)this["DataRetentionDays"];
            set { this["DataRetentionDays"] = value; }
        }


        [ConfigurationProperty("OutputRoot", IsRequired = true)]
        public String OutputRoot
        {
            get => (String)this["OutputRoot"];
            set
            {
                this["OutputRoot"] = value;
            }
        }


        [ConfigurationProperty("PurgeSpecs")]
        public FileSpecElementCollection PurgeSpecs
        {
            get { return ((FileSpecElementCollection)(this["PurgeSpecs"])); }
            set { this["PurgeSpecs"] = value; }
        }

    }
}
