using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.Log4NetOutput
{
    public class FileSpecElement : ConfigurationElement
    {
        [ConfigurationProperty("MatchSpec", IsRequired = true)]
        public String MatchSpec
        {
            get => (String)base["MatchSpec"];
            set => base["MatchSpec"] = value;
        }
    }
}
