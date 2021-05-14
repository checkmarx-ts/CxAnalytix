using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Out.AMQPOutput.Config;
using CxAnalytix.Utilities.DictFilters;
using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using static CxAnalytix.Out.AMQPOutput.Config.AmqpFieldCollection;
using System.Text;
using Newtonsoft.Json;
using CxAnalytix.Utilities.Json;

namespace CxAnalytix.Out.AMQPOutput
{
	class RecordHandler : IRecordRef
	{
		private string _recordName;

		public string RecordName => _recordName;

		private String _exchange;
		private String _topic;
		private DictionaryFilter<String, object> _filter = null;

		public RecordHandler(String recordName)
		{
			_recordName = recordName;

			var cfg = Configuration.Config.GetConfig<AmqpConfig>(AmqpConfig.SECTION_NAME);

			_exchange = cfg.Exchange;
			_topic = recordName;
			var record_cfg = cfg.Records[recordName];

			if (record_cfg != null)
			{
				if (!String.IsNullOrEmpty(record_cfg.Exchange))
					_exchange = record_cfg.Exchange;

				if (!String.IsNullOrEmpty(record_cfg.TopicSpec))
					_topic = record_cfg.TopicSpec;

				if (record_cfg.Filter != null)
				{
					List<String> fnames = new List<string>();

					foreach (AmqpField f in record_cfg.Filter.Fields)
						fnames.Add(f.Name);

					_filter = DictionaryFilterFactory.CreateFilter<string, object>(record_cfg.Filter.Mode, fnames);
				}
			}	
		}


		public void write(IModel channel, IDictionary<string, object> record)
		{
			var dict = record;

			if (_filter != null)
				dict = _filter.Filter(record);

			channel.BasicPublish(_exchange, _topic, null, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dict, Defs.serializerSettings) ) );
		}
	}
}
