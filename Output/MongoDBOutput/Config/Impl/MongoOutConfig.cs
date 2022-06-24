using System;
using System.Configuration;

namespace CxAnalytix.Out.MongoDBOutput.Config.Impl
{
    public class MongoOutConfig : ConfigurationSection
    {

        public MongoOutConfig() { }


        [ConfigurationProperty("GeneratedShardKeys", IsDefaultCollection = false, IsRequired = false)]
        [ConfigurationCollection(typeof(ShardKeySpecConfig), AddItemName = "Spec")]
        public ShardKeySpecConfig ShardKeys
        {
            get
            {
                return (ShardKeySpecConfig)this["GeneratedShardKeys"];
            }

            set
            {
                this["GeneratedShardKeys"] = value;
            }
        }

    }
}
