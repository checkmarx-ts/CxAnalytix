using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Utils;
using System;
using System.Composition;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
    [Export(typeof(ICxFilter))]
    public class CxFilter : MEFableConfigurationSection, ICxFilter
	{

        public CxFilter() { }

        [ImportingConstructor]
        public CxFilter(IConfigSectionResolver resolver) : base(resolver) { }

        [ConfigurationProperty("Team", IsRequired = false)]
        public String TeamRegex
        {
            get => (String)Instance<CxFilter>()["Team"];
            set { Instance<CxFilter>()["Team"] = value; }
        }

        [ConfigurationProperty("Project", IsRequired = false)]
        public String ProjectRegex
        {
            get => (String)Instance<CxFilter>()["Project"];
            set { Instance<CxFilter>()["Project"] = value; }
        }
    }
}

