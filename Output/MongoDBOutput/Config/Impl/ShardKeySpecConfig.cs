using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.MongoDBOutput.Config.Impl
{
    public class ShardKeySpecConfig : ConfigurationElementCollection
    {
        private Dictionary<string, ConfigurationElement> _lookup = new Dictionary<string, ConfigurationElement>();


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

        public new ShardKeySpec this[string collectionName]
        {
            get => _lookup.ContainsKey(collectionName) ? (ShardKeySpec)_lookup[collectionName] : null;
        }
    }
}
