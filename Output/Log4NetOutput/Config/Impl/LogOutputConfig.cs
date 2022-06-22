using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Utils;
using CxAnalytix.Out.Log4NetOutput.Config.Contracts;
using System;
using System.Composition;
using System.Configuration;

namespace CxAnalytix.Out.Log4NetOutput.Config.Impl
{

    [Export(typeof(ILogOutputConfig))]
    public sealed class LogOutputConfig : MEFableConfigurationSection, ILogOutputConfig
    {
        public LogOutputConfig() { }

        [ImportingConstructor]
        public LogOutputConfig(IConfigSectionResolver resolver) : base(resolver) { }


        [ConfigurationProperty("DataRetentionDays", IsRequired = true)]
        public int DataRetentionDays
        {
            get => (int)Instance<LogOutputConfig>()["DataRetentionDays"];
            set { Instance<LogOutputConfig>()["DataRetentionDays"] = value; }
        }


        [ConfigurationProperty("OutputRoot", IsRequired = true)]
        public String OutputRoot
        {
            get => (String)Instance<LogOutputConfig>()["OutputRoot"];
            set
            {
                Instance<LogOutputConfig>()["OutputRoot"] = value;
            }
        }


        [ConfigurationProperty("PurgeSpecs")]
        public FileSpecElementCollection PurgeSpecs
        {
            get { return ((FileSpecElementCollection)(Instance<LogOutputConfig>()["PurgeSpecs"])); }
            set { Instance<LogOutputConfig>()["PurgeSpecs"] = value; }
        }

    }
}
