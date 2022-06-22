using CxAnalytix.Out.AMQPOutput.Config.Impls;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput.Config
{
	public class AmqpRecordConfigCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new AmqpRecordConfig();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return (element as AmqpRecordConfig).RecordName;
		}

		public new AmqpRecordConfig this[string propertyName] => (AmqpRecordConfig)BaseGet(propertyName);

	}
}
