using CxAnalytix.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput.Config
{
	public class AmqpRecordHeaderConfig : EnvAwareConfigurationElement
	{
		[ConfigurationProperty("Key", IsRequired = true, IsKey = true)]
		public String HeaderKey
		{
			get => (String)base["Key"];
			set => base["Key"] = value;
		}

		[ConfigurationProperty("Spec", IsRequired = true)]
		public String HeaderValueSpec
		{
			get => (String)base["Spec"];
			set => base["Spec"] = value;
		}

	}
}
