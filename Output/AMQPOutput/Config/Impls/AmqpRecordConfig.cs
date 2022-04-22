using CxAnalytix.Configuration.Utils;
using System;
using System.Configuration;

namespace CxAnalytix.Out.AMQPOutput.Config.Impls
{
	public class AmqpRecordConfig : EnvAwareConfigurationElement
	{

		[ConfigurationProperty("Name", IsRequired = true, IsKey = true)]
		public String RecordName
		{
			get => (String)base["Name"];
			set => base["Name"] = value;
		}

		[ConfigurationProperty("Exchange", IsRequired = false, DefaultValue = null)]
		public String Exchange
		{
			get => (String)base["Exchange"];
			set => base["Exchange"] = value;
		}

		[ConfigurationProperty("TopicSpec", IsRequired = false, DefaultValue = null)]
		public String TopicSpec
		{
			get => (String)base["TopicSpec"];
			set => base["TopicSpec"] = value;
		}


		[ConfigurationProperty("Filter", IsRequired = false, DefaultValue = null)]
		public AmqpRecordFilterConfig Filter
		{
			get => (AmqpRecordFilterConfig)base["Filter"];
			set => base["Filter"] = value;
		}

		[ConfigurationProperty("MessageHeaders", IsRequired = false, DefaultValue = null)]
		[ConfigurationCollection(typeof(AmqpRecordHeaderConfig), AddItemName = "Header")]
		public AmqpRecordHeaderCollection Headers
		{
			get => (AmqpRecordHeaderCollection)base["MessageHeaders"];
			set => base["MessageHeaders"] = value;
		}



	}
}
