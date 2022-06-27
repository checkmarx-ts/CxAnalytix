using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Impls
{
    public class EnabledTransformersCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
            return new EnabledTransformer();
        }

		protected override object GetElementKey(ConfigurationElement element)
		{
			return (element as EnabledTransformer).Name;
		}

	}
}
