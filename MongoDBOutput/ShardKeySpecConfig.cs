using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
    public class ShardKeySpecConfig : ConfigurationElementCollection
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new ShardKeySpec();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ShardKeySpec)element).Collection;
        }
    }
}
