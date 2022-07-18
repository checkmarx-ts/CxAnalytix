using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput.Config
{
	public class AmqpRecordHeaderCollection : ConfigurationElementCollection
	{
		protected override ConfigurationElement CreateNewElement()
		{
			return new AmqpRecordHeaderConfig();
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			if (element == null)
				return null;

			return (element as AmqpRecordHeaderConfig).HeaderKey;
		}

		public new AmqpRecordHeaderConfig this[string propertyName] => (AmqpRecordHeaderConfig)BaseGet(propertyName);

	}
}
