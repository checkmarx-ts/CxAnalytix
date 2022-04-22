using CxAnalytix.Configuration.Utils;
using CxAnalytix.Out.AMQPOutput.Config.Contracts;
using System;
using System.Composition;
using System.Configuration;

namespace CxAnalytix.Out.AMQPOutput.Config.Impls
{

	[SecureConfigSection(SensitiveStringProp = "Password")]
	[Export(typeof(IAmqpConnectionConfig))]
	internal class AmqpConnectionConfig : EnvAwareConfigurationSection, IAmqpConnectionConfig
	{

		public AmqpConnectionConfig()
		{
			CxAnalytix.Configuration.Impls.Config.AutoInit(this);
		}


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
