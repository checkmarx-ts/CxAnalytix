using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Utils;
using CxAnalytix.Out.MongoDBOutput.Config.Contracts;
using System;
using System.Composition;
using System.Configuration;

namespace CxAnalytix.Out.MongoDBOutput.Config.Impl
{
    [Export(typeof(IMongoOutConfig))]
    public class MongoOutConfig : MEFableConfigurationSection, IMongoOutConfig
    {

        public MongoOutConfig() { }

        [ImportingConstructor]
        public MongoOutConfig(IConfigSectionResolver resolver) : base(resolver) { }


        [ConfigurationProperty("GeneratedShardKeys", IsDefaultCollection = false, IsRequired = false)]
        [ConfigurationCollection(typeof(ShardKeySpecConfig),
            AddItemName = "Spec")]
        public ShardKeySpecConfig ShardKeys
        {
            get
            {
                return (ShardKeySpecConfig)Instance<MongoOutConfig>()["GeneratedShardKeys"];
            }

            set
            {
                Instance<MongoOutConfig>()["GeneratedShardKeys"] = value;
            }
        }

    }
}
