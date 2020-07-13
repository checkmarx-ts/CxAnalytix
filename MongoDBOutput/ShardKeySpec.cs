using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
    public class ShardKeySpec : ConfigurationElement
    {


        [ConfigurationProperty("KeyName", IsRequired = true)]
        public String Key
        {
            get => (String)this["KeyName"];
            set
            {
                this["KeyName"] = value;
            }
        }


        [ConfigurationProperty("CollectionName", IsRequired = true)]
        public String Collection
        {
            get => (String)this["CollectionName"];
            set
            {
                this["CollectionName"] = value;
            }
        }


        [ConfigurationProperty("FormatSpec", IsRequired = true)]
        public String Format
        {
            get => (String)this["FormatSpec"];
            set
            {
                this["FormatSpec"] = value;
            }
        }

        [ConfigurationProperty("NoHash", IsRequired = false)]
        public bool NoHash
        {
            get => (bool)this["NoHash"];
            set
            {
                this["NoHash"] = value;
            }
        }


        public override string ToString()
        {
            return String.Format("Collection [{0}] Key [{1}] FormatSpec [{2}] [{3}]", Collection, Key, Format, (!NoHash) ? ("HASHED") : ("RAW") );
        }
    }
}
