﻿using System;
using CxAnalytix.Out.AMQPOutput.Config.Impls;
using System.Configuration;
using static CxAnalytix.Utilities.DictFilters.DictionaryFilterFactory;

namespace CxAnalytix.Out.AMQPOutput.Config
{
	public class AmqpRecordFilterConfig : ConfigurationElement
	{

		[ConfigurationProperty("Mode", IsRequired = true, DefaultValue = FilterModes.None)]
		public FilterModes Mode
		{
			get => (FilterModes)base["Mode"];
			set => base["Mode"] = value;
		}

		[ConfigurationProperty("Fields", IsDefaultCollection = false, IsRequired = true)]
		[ConfigurationCollection(typeof(AmqpFieldCollection.AmqpField), AddItemName = "Field")]
		public AmqpFieldCollection Fields
		{
			get
			{
				return (AmqpFieldCollection)base["Fields"];
			}

			set
			{
				base["Fields"] = value;
			}
		}


	}
}
