using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Impls
{
    public class EnabledTransformer : ConfigurationElement
    {

		[ConfigurationProperty("Name", IsRequired = true)]
		public String Name
		{
			get => (String)base["Name"];
			set => base["Name"] = value;
		}

	}
}
