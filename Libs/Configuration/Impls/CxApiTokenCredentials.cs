using CxAnalytix.Configuration.Utils;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Impls
{

    [SecureConfigSection("Token")]
    public class CxApiTokenCredentials : EnvAwareConfigurationSection
    {
        [ConfigurationProperty("Tenant", IsRequired = true)]
        public String Tenant
        {
            get => (String)this["Tenant"];
            set { this["Tenant"] = value; }
        }


        [ConfigurationProperty("Token", IsRequired = true)]
        public String Token
        {
            get => (String)this["Token"];
            set { this["Token"] = value; }
        }

    }
}
