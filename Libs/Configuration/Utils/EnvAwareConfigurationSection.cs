using System;
using System.Configuration;


namespace CxAnalytix.Configuration.Utils
{
    public class EnvAwareConfigurationSection : ConfigurationSection
    {
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
