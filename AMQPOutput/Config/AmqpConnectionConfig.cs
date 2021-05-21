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


		[ConfigurationProperty("UserName", IsRequired = false)]
		public String UserName
		{
			get
			{
				return (String)base["UserName"];
			}

			set
			{
				base["UserName"] = value;
			}
		}


		[ConfigurationProperty("Password", IsRequired = false)]
		public String Password
		{
			get
			{
				return (String)base["Password"];
			}

			set
			{
				base["Password"] = value;
			}
		}



		[ConfigurationProperty("ClusterNodes", IsDefaultCollection = false, IsRequired = true)]
		[ConfigurationCollection(typeof(AmqpEndpointCollection), AddItemName = "Endpoint")]
		public AmqpEndpointCollection Endpoints
		{
			get
			{
				return (AmqpEndpointCollection)base["ClusterNodes"];
			}

			set
			{
				base["ClusterNodes"] = value;
			}
		}

	}
}
