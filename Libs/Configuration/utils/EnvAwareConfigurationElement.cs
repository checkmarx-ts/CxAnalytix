using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Configuration.Utils
{
	public class EnvAwareConfigurationElement : ConfigurationElement
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
