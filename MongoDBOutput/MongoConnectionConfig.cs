using CxAnalytix.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{

    [SecureConfigSection(SensitiveStringProp = "ConnectionString")]
    public class MongoConnectionConfig : EnvAwareConfigurationSection
    {

        public static readonly String SECTION_NAME = "CxMongoConnection";

        [ConfigurationProperty("ConnectionString", IsRequired = true)]
        public String ConnectionString
        {
            get => (String)this["ConnectionString"];
            set
            {
                this["ConnectionString"] = value;
            }
        }
    }
}
