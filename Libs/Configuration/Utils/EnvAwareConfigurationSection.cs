using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Impls;
using System;
using System.Composition;
using System.Configuration;


namespace CxAnalytix.Configuration.Utils
{
    public class EnvAwareConfigurationSection : MEFableConfigurationSection
    {

        public EnvAwareConfigurationSection() {}

        public EnvAwareConfigurationSection(IConfigSectionResolver resolver) : base(resolver) {}

        protected new object this[string propertyName]
        {
            get
            {
                object retVal = base[propertyName];

                if (retVal.GetType() == typeof(String))
                    return Environment.ExpandEnvironmentVariables((String)retVal);

                return retVal;
            }

            set
            {
                base[propertyName] = value;
            }
        }
    }
}
