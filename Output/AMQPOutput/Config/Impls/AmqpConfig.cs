using CxAnalytix.Configuration.Contracts;
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

		public AmqpConfig() {}

		[ImportingConstructor]
		public AmqpConfig(IConfigSectionResolver resolver) : base(resolver) { }


		[ConfigurationProperty("DefaultExchange", IsRequired = true)]
		public String Exchange
		{
			get => (String)Instance<AmqpConfig>()["DefaultExchange"];
			set => Instance<AmqpConfig>()["DefaultExchange"] = value;
		}


		[ConfigurationProperty("RecordSpecs", IsDefaultCollection = false, IsRequired = false)]
		[ConfigurationCollection(typeof(AmqpRecordConfig), AddItemName = "Record")]
		public AmqpRecordConfigCollection Records
		{
			get
			{
				return (AmqpRecordConfigCollection)Instance<AmqpConfig>()["RecordSpecs"];
			}

			set
			{
				Instance<AmqpConfig>()["RecordSpecs"] = value;
			}
		}

	}
}
