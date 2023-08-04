using CxAnalytix.Configuration.Utils;
using System;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
    public sealed class CxSASTConnection : CxConnection
    {

        [ConfigurationProperty("mnoURL", IsRequired = false)]
        public String MNOUrl
        {
            get => (String)this["mnoURL"];
            set { this["mnoURL"] = value; }
        }

        [ConfigurationProperty("UseOdata", IsRequired = false)]
        public CxSASTAPIOverrides Overrides
        {
            get => (CxSASTAPIOverrides)this["UseOdata"];
            set => this["UseOdata"] = value;
        }
	}
}
