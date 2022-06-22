using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput.Config.Impl
{
    public class ShardKeySpec : ConfigurationElement
    {


        [ConfigurationProperty("KeyName", IsRequired = true)]
        public string Key
        {
            get => (string)this["KeyName"];
            set
            {
                this["KeyName"] = value;
            }
        }


        [ConfigurationProperty("CollectionName", IsRequired = true)]
        public string Collection
        {
            get => (string)this["CollectionName"];
            set
            {
                this["CollectionName"] = value;
            }
        }


        [ConfigurationProperty("FormatSpec", IsRequired = true)]
        public string Format
        {
            get => (string)this["FormatSpec"];
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
            return string.Format("Collection [{0}] Key [{1}] FormatSpec [{2}] [{3}]", Collection, Key, Format, !NoHash ? "HASHED" : "RAW");
        }
    }
}
