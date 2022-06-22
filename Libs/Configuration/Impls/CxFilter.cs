using CxAnalytix.Configuration.Contracts;
using System;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
    internal class CxFilter : ConfigurationSection, ICxFilter
	{
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

