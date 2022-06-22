using CxAnalytix.Configuration.Contracts;
using System;
using System.Composition;
using System.Configuration;

namespace CxAnalytix.Configuration.Impls
{
    [Export(typeof(IConfigSectionResolver))]
    public class ConfigSectionResolver : IConfigSectionResolver
    {
        public ConfigurationSection GetSectionInstance(Type t)
        {
            ConfigurationSectionCollection mgr = Config.Sections;

            foreach(ConfigurationSection s in mgr)
            {
                if (s.GetType() == t)
                    return s;

            }

            return null;
        }
    }
}
