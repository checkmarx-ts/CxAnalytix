using CxAnalytix.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput.Config
{

	[SecureConfigSection]
	public class AmqpConnectionConfig : EnvAwareConfigurationSection
	{

		public static readonly String SECTION_NAME = "AMQPConnection";



		[ConfigurationProperty("Servers", IsDefaultCollection = false, IsRequired = true)]
		[ConfigurationCollection(typeof(AmqpEndpointCollection), AddItemName = "Endpoint")]
		public AmqpEndpointCollection Endpoints
		{
			get
			{
				return (AmqpEndpointCollection)base["Servers"];
			}

			set
			{
				base["Servers"] = value;
			}
		}

	}
}
