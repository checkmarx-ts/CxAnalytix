﻿using System;
using System.Configuration;

namespace CxAnalytix.Out.MongoDBOutput
{
    public class MongoOutConfig : ConfigurationSection
    {
        public static readonly String SECTION_NAME = "CxMongoOutput";
        
        [ConfigurationProperty("ConnectionString", IsRequired = false)]
        public String ConnectionString
        {
            get => (String)this["ConnectionString"];
            set
            {
                this["ConnectionString"] = value;
            }
        }

        [ConfigurationProperty("UseTransactions", IsRequired = false, DefaultValue = false)]
        public bool UseTransactions
        {
            get => (bool)this["UseTransactions"];
            set
            {
                this["UseTransactions"] = value;
            }
        }


        [ConfigurationProperty("TransactionTimeoutSeconds", IsRequired = false, DefaultValue = 60)]
        public int TrxTimeoutSecs
        {
            get => (int)this["TransactionTimeoutSeconds"];
            set
            {
                this["TransactionTimeoutSeconds"] = value;
            }
        }


        [ConfigurationProperty("GeneratedShardKeys", IsDefaultCollection = false, IsRequired = false)]
        [ConfigurationCollection(typeof(ShardKeySpecConfig),
            AddItemName = "Spec")]
        public ShardKeySpecConfig ShardKeys
        {
            get
            {
                return (ShardKeySpecConfig)base["GeneratedShardKeys"];
            }

            set
            {
                base["GeneratedShardKeys"] = value;
            }
        }

    }
}
