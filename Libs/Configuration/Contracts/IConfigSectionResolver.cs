using System;
using System.Configuration;

namespace CxAnalytix.Configuration.Contracts
{
    public interface IConfigSectionResolver
    {
        ConfigurationSection GetSectionInstance(Type t);

    }
}
