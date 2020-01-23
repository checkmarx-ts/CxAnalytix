using CxAnalytics.Configuration;
using log4net;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytics.Out.Log4NetOutput
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


        [ConfigurationProperty("OutputDirectory", IsRequired = true)]
        public String OutputDirectory
        {
            get => (String)this["OutputDirectory"];
            set
            {
                this["OutputDirectory"] = value;
            }
        }

    }
}
