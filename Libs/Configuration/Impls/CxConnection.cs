using CxAnalytix.Configuration.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Impls
{
    public class CxConnection : EnvAwareConfigurationSection
    {
        [ConfigurationProperty("URL", IsRequired = true)]
        public String URL
        {
            get => (String)this["URL"];
            set { this["URL"] = value; }
        }

        [ConfigurationProperty("TimeoutSeconds", IsRequired = false, DefaultValue = 300)]
        public int TimeoutSeconds
        {
            get => (int)this["TimeoutSeconds"];
            set { this["TimeoutSeconds"] = value; }
        }

        [ConfigurationProperty("ValidateCertificates", IsRequired = false, DefaultValue=true)]
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
