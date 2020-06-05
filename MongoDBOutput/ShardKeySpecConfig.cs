using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput
{
    public class ShardKeySpecConfig : ConfigurationElementCollection
    {
        private Dictionary<String, ConfigurationElement> _lookup = new Dictionary<string, ConfigurationElement>();


        protected override ConfigurationElement CreateNewElement()
        {
            return new ShardKeySpec();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((ShardKeySpec)element).Collection;
        }

        protected override void BaseAdd(ConfigurationElement element)
        {
            base.BaseAdd(element);
            _lookup.Add(((ShardKeySpec)element).Collection, element);
        }

        public new ShardKeySpec this[String collectionName]
        {
            get => (_lookup.ContainsKey(collectionName)) ? ((ShardKeySpec)_lookup[collectionName]) : (null); 
        }
    }
}
