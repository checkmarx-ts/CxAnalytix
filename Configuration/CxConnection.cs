using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytics.Configuration
{
    public sealed class CxConnection : ConfigurationSection
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
