using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Configuration
{
    public sealed class CxConnection : EnvAwareConfigurationSection
    {
        internal CxConnection ()
        {

        }

        public static readonly String SECTION_NAME = "CxConnection";

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

    }
}
