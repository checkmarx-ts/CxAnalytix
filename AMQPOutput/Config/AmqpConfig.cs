using CxAnalytix.Configuration;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput.Config
{
	public class AmqpConfig : EnvAwareConfigurationSection
	{
		public static readonly String SECTION_NAME = "AMQPConfig";

		[ConfigurationProperty("DefaultExchange", IsRequired = true)]
		public String Exchange
		{
			get => (String)base["DefaultExchange"];
			set => base["DefaultExchange"] = value;
		}


		// TODO: Default exchange binding args



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
