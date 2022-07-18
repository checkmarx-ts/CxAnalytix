using CxAnalytix.Configuration.Utils;
using System;
using System.Configuration;

namespace CxAnalytix.Out.AMQPOutput.Config.Impls
{
	internal class AmqpConfig : EnvAwareConfigurationSection
	{

		public AmqpConfig() {}

		[ConfigurationProperty("DefaultExchange", IsRequired = true)]
		public String Exchange
		{
			get => (String)this["DefaultExchange"];
			set => this["DefaultExchange"] = value;
		}


		[ConfigurationProperty("RecordSpecs", IsDefaultCollection = false, IsRequired = false)]
		[ConfigurationCollection(typeof(AmqpRecordConfig), AddItemName = "Record")]
		public AmqpRecordConfigCollection Records
		{
			get
			{
				return (AmqpRecordConfigCollection)this["RecordSpecs"];
			}

			set
			{
				this["RecordSpecs"] = value;
			}
		}

	}
}
