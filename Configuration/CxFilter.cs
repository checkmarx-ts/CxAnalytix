using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Configuration
{
	public class CxFilter : ConfigurationSection
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

