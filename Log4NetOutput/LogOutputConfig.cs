using CxAnalytix.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.Log4NetOutput
{
    public sealed class LogOutputConfig : ConfigurationSection
    {
        public static readonly String SECTION_NAME = "CxLogOutput";

        internal LogOutputConfig ()
        {
        }


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
            get { return ((FileSpecElementCollection)(base["PurgeSpecs"])); }
            set { base["PurgeSpecs"] = value; }
        }

    }
}
