using System;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
    public class CxFilter : ConfigurationSection
	{

        public CxFilter() { }

        [ConfigurationProperty("Team", IsRequired = false)]
        public String TeamRegex
        {
            get => (String)this["Team"];
            set { this["Team"] = value; }
        }

        [ConfigurationProperty("Project", IsRequired = false)]
        public String ProjectRegex
        {
            get => (String)this["Project"];
            set { this["Project"] = value; }
        }
    }
}

