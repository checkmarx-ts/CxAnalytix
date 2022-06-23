using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Out.AMQPOutput.Config;
using CxAnalytix.Utilities.DictFilters;
using CxAnalytix.Extensions;
using System;
using System.Collections.Generic;
using RabbitMQ.Client;
using System.Text;
using Newtonsoft.Json;
using CxAnalytix.Utilities.Json;
using CxAnalytix.Out.AMQPOutput.Config.Impls;
using static CxAnalytix.Out.AMQPOutput.Config.Impls.AmqpFieldCollection;
using System.Reflection;
using CxAnalytix.Out.AMQPOutput.Config.Contracts;
using System.Composition;

namespace CxAnalytix.Out.AMQPOutput
{
	class RecordHandler : IRecordRef
	{
		private string _recordName;

		public string RecordName => _recordName;

		private String _exchange;
		private String _topic;
		private DictionaryFilter<String, object> _filter = null;
		private Dictionary<String, String> _headers = null;

		private static int MAX_ROUTING_KEY_SIZE = 255;
		private static String GENERATION_KEY = "Generation";

		[Import]
		private IAmqpConfig _cfg {get; set;}


		public RecordHandler(String recordName)
		{
			CxAnalytix.Configuration.Impls.Config.InjectConfigs(this);



			_recordName = recordName;

			_exchange = _cfg.Exchange;
			_topic = recordName;
			var record_cfg = _cfg.Records[recordName];


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


				if (record_cfg.Headers != null)
				{
					_headers = new Dictionary<string, string>();
					foreach (AmqpRecordHeaderConfig header in record_cfg.Headers)
						if (header.HeaderKey.CompareTo(GENERATION_KEY) != 0)
							_headers.Add(header.HeaderKey, header.HeaderValueSpec);
				}
			}
		}


		public void write(IModel channel, IDictionary<string, object> record)
		{
			var dict = record;

			if (_filter != null)
				dict = _filter.Filter(record);

			var props = channel.CreateBasicProperties();
			props.ContentType = "application/json";
			props.Type = _recordName;

			props.Headers = new Dictionary<String, Object>();
			props.Headers.Add(GENERATION_KEY, 0);

			if (_headers != null)
				foreach (var headerSpec in _headers)
					props.Headers.Add(headerSpec.Key, record.ComposeString(headerSpec.Value));


			channel.BasicPublish(_exchange, record.ComposeString(_topic).Truncate(MAX_ROUTING_KEY_SIZE), props, Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(dict, Defs.serializerSettings) ) );
		}
	}
}
