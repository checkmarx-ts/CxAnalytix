using CxAnalytix.Configuration.Utils;
using System;
using System.Configuration;

namespace CxAnalytix.Out.AMQPOutput.Config.Impls
{

	[SecureConfigSection("Password")]
	internal class AmqpConnectionConfig : EnvAwareConfigurationSection
	{

		public AmqpConnectionConfig()
		{
		}

		[ConfigurationProperty("UserName", IsRequired = false)]
		public String UserName
		{
			get
			{
				return (String)this["UserName"];
			}

			set
			{
				this["UserName"] = value;
			}
		}


		[ConfigurationProperty("Password", IsRequired = false)]
		public String Password
		{
			get
			{
				return (String)this["Password"];
			}

			set
			{
				this["Password"] = value;
			}
		}



		[ConfigurationProperty("ClusterNodes", IsDefaultCollection = false, IsRequired = true)]
		[ConfigurationCollection(typeof(AmqpEndpointCollection), AddItemName = "Endpoint")]
		public AmqpEndpointCollection Endpoints
		{
			get
			{
				return (AmqpEndpointCollection)this["ClusterNodes"];
			}

			set
			{
				this["ClusterNodes"] = value;
			}
		}

	}
}
