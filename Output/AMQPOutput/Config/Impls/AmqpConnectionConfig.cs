﻿using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Utils;
using CxAnalytix.Out.AMQPOutput.Config.Contracts;
using System;
using System.Composition;
using System.Configuration;

namespace CxAnalytix.Out.AMQPOutput.Config.Impls
{

	[SecureConfigSection("Password")]
	[Export(typeof(IAmqpConnectionConfig))]
	internal class AmqpConnectionConfig : EnvAwareConfigurationSection, IAmqpConnectionConfig
	{

		public AmqpConnectionConfig()
		{
		}

		[ImportingConstructor]
		public AmqpConnectionConfig(IConfigSectionResolver resolver) : base(resolver) { }


		[ConfigurationProperty("UserName", IsRequired = false)]
		public String UserName
		{
			get
			{
				return (String)Instance<AmqpConnectionConfig>()["UserName"];
			}

			set
			{
				Instance<AmqpConnectionConfig>()["UserName"] = value;
			}
		}


		[ConfigurationProperty("Password", IsRequired = false)]
		public String Password
		{
			get
			{
				return (String)Instance<AmqpConnectionConfig>()["Password"];
			}

			set
			{
				Instance<AmqpConnectionConfig>()["Password"] = value;
			}
		}



		[ConfigurationProperty("ClusterNodes", IsDefaultCollection = false, IsRequired = true)]
		[ConfigurationCollection(typeof(AmqpEndpointCollection), AddItemName = "Endpoint")]
		public AmqpEndpointCollection Endpoints
		{
			get
			{
				return (AmqpEndpointCollection)Instance<AmqpConnectionConfig>()["ClusterNodes"];
			}

			set
			{
				Instance<AmqpConnectionConfig>()["ClusterNodes"] = value;
			}
		}

	}
}