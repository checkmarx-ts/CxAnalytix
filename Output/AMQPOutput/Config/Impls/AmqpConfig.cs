using CxAnalytix.Configuration.Utils;
using CxAnalytix.Out.AMQPOutput.Config.Contracts;
using System;
using System.Composition;
using System.Configuration;

namespace CxAnalytix.Out.AMQPOutput.Config.Impls
{
	[Export(typeof(IAmqpConfig))]
	internal class AmqpConfig : EnvAwareConfigurationSection, IAmqpConfig
	{

		public AmqpConfig()
		{
			CxAnalytix.Configuration.Impls.Config.AutoInit(this);
		}


		[ConfigurationProperty("DefaultExchange", IsRequired = true)]
		public String Exchange
		{
			get => (String)base["DefaultExchange"];
			set => base["DefaultExchange"] = value;
		}


		[ConfigurationProperty("RecordSpecs", IsDefaultCollection = false, IsRequired = false)]
		[ConfigurationCollection(typeof(AmqpRecordConfig), AddItemName = "Record")]
		public AmqpRecordConfigCollection Records
		{
			get
			{
				return (AmqpRecordConfigCollection)base["RecordSpecs"];
			}

			set
			{
				base["RecordSpecs"] = value;
			}
		}

	}
}
