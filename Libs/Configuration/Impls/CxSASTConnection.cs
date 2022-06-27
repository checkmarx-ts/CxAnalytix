using CxAnalytix.Configuration.Utils;
using System;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
    public sealed class CxSASTConnection : EnvAwareConfigurationSection
    {
        public CxSASTConnection() { }


        [ConfigurationProperty("URL", IsRequired = true)]
        public String URL
        {
            get => (String)this["URL"];
            set { this["URL"] = value; }
        }

        [ConfigurationProperty("mnoURL", IsRequired = false)]
        public String MNOUrl
        {
            get => (String)this["mnoURL"];
            set { this["mnoURL"] = value; }
        }

        [ConfigurationProperty("TimeoutSeconds", IsRequired = true)]
        public int TimeoutSeconds
        {
            get => (int)this["TimeoutSeconds"];
            set { this["TimeoutSeconds"] = value; }
        }

        [ConfigurationProperty("ValidateCertificates", IsRequired = true)]
        public bool ValidateCertificates
        {
            get => (bool)this["ValidateCertificates"];
            set { this["ValidateCertificates"] = value; }
        }

        [ConfigurationProperty("RetryLoop", IsRequired = false, DefaultValue = 0)]
        public int RetryLoop
        {
            get => (int)this["RetryLoop"];
            set { this["RetryLoop"] = value; }
        }
	}
}
