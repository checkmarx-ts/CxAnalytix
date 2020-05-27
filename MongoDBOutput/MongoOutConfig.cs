using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
    public class MongoOutConfig : ConfigurationSection
    {
        public static readonly String SECTION_NAME = "CxMongoOutput";
        
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
