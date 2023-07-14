using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Impls
{

    public interface IKeyProducer
    {
        String Key { get; }
    }

    public class ConfigElementCollection<T> : ConfigurationElementCollection where T : ConfigurationElement, IKeyProducer, new()
    {
        protected override ConfigurationElement CreateNewElement()
        {
            return new T();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                return null;

            return (element as IKeyProducer).Key;
        }

    }
}
