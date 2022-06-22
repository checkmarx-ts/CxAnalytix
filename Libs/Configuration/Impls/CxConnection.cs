using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Utils;
using System;
using System.Composition;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
    [Export(typeof(ICxConnection))]
    public sealed class CxConnection : EnvAwareConfigurationSection, ICxConnection
    {
        public CxConnection() { }

        [ImportingConstructor]
        public CxConnection(IConfigSectionResolver resolver) : base(resolver) { }

        [ConfigurationProperty("URL", IsRequired = true)]
        public String URL
        {
            get => (String)Instance<CxConnection>()["URL"];
            set { Instance<CxConnection>()["URL"] = value; }
        }

        [ConfigurationProperty("mnoURL", IsRequired = false)]
        public String MNOUrl
        {
            get => (String)Instance<CxConnection>()["mnoURL"];
            set { Instance<CxConnection>()["mnoURL"] = value; }
        }

        [ConfigurationProperty("TimeoutSeconds", IsRequired = true)]
        public int TimeoutSeconds
        {
            get => (int)Instance<CxConnection>()["TimeoutSeconds"];
            set { Instance<CxConnection>()["TimeoutSeconds"] = value; }
        }

        [ConfigurationProperty("ValidateCertificates", IsRequired = true)]
        public bool ValidateCertificates
        {
            get => (bool)Instance<CxConnection>()["ValidateCertificates"];
            set { Instance<CxConnection>()["ValidateCertificates"] = value; }
        }

        [ConfigurationProperty("RetryLoop", IsRequired = false, DefaultValue = 0)]
        public int RetryLoop
        {
            get => (int)Instance<CxConnection>()["RetryLoop"];
            set { Instance<CxConnection>()["RetryLoop"] = value; }
        }
	}
}
