using CxAnalytix.Configuration;
using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Utils;
using CxAnalytix.Out.MongoDBOutput.Config.Contracts;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput.Config.Impl
{

    [Export(typeof(IMongoConnectionConfig))]
    [SecureConfigSection("ConnectionString")]
    public class MongoConnectionConfig : EnvAwareConfigurationSection, IMongoConnectionConfig
    {

        public MongoConnectionConfig() { }

        [ImportingConstructor]
        public MongoConnectionConfig(IConfigSectionResolver resolver) : base(resolver) { }

        [ConfigurationProperty("ConnectionString", IsRequired = true)]
        public string ConnectionString
        {
            get => (string)Instance<MongoConnectionConfig>()["ConnectionString"];
            set
            {
                Instance<MongoConnectionConfig>()["ConnectionString"] = value;
            }
        }
    }
}
