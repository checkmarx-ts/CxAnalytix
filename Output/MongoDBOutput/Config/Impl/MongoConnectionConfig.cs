using CxAnalytix.Configuration;
using CxAnalytix.Configuration.Utils;
using System;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput.Config.Impl
{

    [SecureConfigSection("ConnectionString")]
    public class MongoConnectionConfig : EnvAwareConfigurationSection
    {

        public MongoConnectionConfig() { }


        [ConfigurationProperty("ConnectionString", IsRequired = true)]
        public string ConnectionString
        {
            get => (string)this["ConnectionString"];
            set
            {
                this["ConnectionString"] = value;
            }
        }
    }
}
