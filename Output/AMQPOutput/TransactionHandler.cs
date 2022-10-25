using CxAnalytix.Configuration.Impls;
using CxAnalytix.Exceptions;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Out.AMQPOutput.Config.Impls;
using CxAnalytix.Utilities.Json;
using log4net;
using Newtonsoft.Json;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Channels;

namespace CxAnalytix.Out.AMQPOutput
{
	class TransactionHandler : IOutputTransaction
	{
		private static ILog _log = LogManager.GetLogger(typeof(TransactionHandler));

		private static readonly String HEADER_SEQUENCE = "Sequence";
        private static readonly String HEADER_TID = "TransactionId";
        private static readonly String HEADER_GROUP = "GroupId";
        private static readonly String HEADER_GROUP_TYPE = "GroupRecordType";
        private static readonly String HEADER_TOTAL = "GroupTotalMsgs";

        private static readonly String MARKER_BEGIN = "BeginTransaction";
        private static readonly String MARKER_END = "EndTransaction";
        private static readonly String MARKER_TYPE = "TransactionMarker";


		private IModel _channel;
        private IModel Channel
		{
			get
			{
				if (_channel != null && _channel.IsClosed)
				{
					_channel.Dispose();
					_channel = AMQPConnection.GetModel();
				}

				return _channel;
			}

			set
			{
				_channel = value;
			}
		}
		private bool _committed = false;
		private bool _noRollback = false;

		AmqpConnectionConfig ConnectionConfig => CxAnalytix.Configuration.Impls.Config.GetConfig<AmqpConnectionConfig>();


		public TransactionHandler()
		{
			Channel = AMQPConnection.GetModel();
			TransactionId = Guid.NewGuid().ToString();
			Channel.ContinuationTimeout = new TimeSpan(0, 0, ConnectionConfig.TimeoutSeconds);
			Channel.TxSelect();
		}


		private struct TransactionData
		{
            public TransactionData()
			{
				Sequence = 0;
				GroupId = Guid.NewGuid().ToString();
            }

			public String GroupId { get; set; }
			public uint Sequence { get; set; }
		}


		private Dictionary<RecordHandler, TransactionData> _sequences = new();

		public string TransactionId { get; internal set; }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public bool Commit()
		{
			try
			{
				foreach(var handler in _sequences.Keys)
                    WriteMarkerToQueue(MARKER_END, handler.Exchange, new Dictionary<String, Object>()
                    {
						{ HEADER_TID, TransactionId }
						,{HEADER_GROUP_TYPE, handler.RecordName}
                        ,{ HEADER_GROUP, _sequences[handler].GroupId }
						,{ HEADER_TOTAL, _sequences[handler].Sequence}

                    });

                Channel.TxCommit();
				_committed = true;
			}
			catch (Exception ex)
			{
				_log.Error($"Error committing transaction {TransactionId}", ex);
				_noRollback = true;
			}

			return _committed;
		}

		public void Dispose()
		{
			if (Channel != null)
			{
				if (!_committed && !_noRollback)
					Channel.TxRollback();

				Channel.Dispose();
				Channel = null;
			}
       
        }

        private Dictionary<String, Object> MakeMessageHeaders(RecordHandler handler)
		{
			uint curSequence = 0;

			lock (_sequences)
			{
				var record = _sequences[handler];

				curSequence = record.Sequence;

				record.Sequence++;

				_sequences[handler] = record;
            }

			return new Dictionary<String, Object>()
            {
                { HEADER_TID, TransactionId }
                ,{HEADER_SEQUENCE, curSequence}
				,{ HEADER_GROUP, _sequences[handler].GroupId}
            };
        }

		private void CheckTransactionStateForHandler(RecordHandler rh)
		{
            if (!_sequences.ContainsKey(rh))
            {
                _sequences.Add(rh, new TransactionData());

				WriteMarkerToQueue(MARKER_BEGIN, rh.Exchange, new Dictionary<String, Object>() {
					{ HEADER_TID, TransactionId }
                    ,{HEADER_GROUP_TYPE, rh.RecordName}
                    ,{ HEADER_GROUP, _sequences[rh].GroupId}
                });
            }
        }


		private void WriteMarkerToQueue(String routingKey, String exchange, Dictionary<String, Object> content)
		{
            var props = Channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.Type = MARKER_TYPE;

            Channel.BasicPublish(exchange, routingKey, props,
                Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(content, Defs.serializerSettings)));
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        public void write(IRecordRef which, IDictionary<string, object> record)
		{
			if (_committed)
				throw new UnrecoverableOperationException($"Attempted to write record {which.RecordName} after commit.");

			if (!(which is RecordHandler))
				throw new UnrecoverableOperationException($"RecordRef for {which.RecordName} is not of type RecordHandler");

			var rh = which as RecordHandler;
			
			CheckTransactionStateForHandler(rh);

            rh.write(Channel, MakeMessageHeaders(rh), record);
		}
	}
}
