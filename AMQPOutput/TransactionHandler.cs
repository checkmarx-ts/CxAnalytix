using CxAnalytix.Exceptions;
using CxAnalytix.Interfaces.Outputs;
using log4net;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput
{
	class TransactionHandler : IOutputTransaction
	{
		private static ILog _log = LogManager.GetLogger(typeof(TransactionHandler));

		private IModel _channel;
		private bool _committed = false;

		public TransactionHandler(IModel amqpChannel)
		{
			_channel = amqpChannel;
			_channel.TxSelect();

		}

		public string TransactionId => $"CHANNEL:{_channel.ChannelNumber}";

		public bool Commit()
		{
			try
			{
				_channel.TxCommit();
				_committed = true;
			}
			catch (Exception ex)
			{
				_log.Error($"Error committing transaction {TransactionId}", ex);
			}

			return _committed;
		}

		public void Dispose()
		{
			if (_channel != null)
			{
				if (!_committed)
					_channel.TxRollback();

				_channel.Dispose();
				_channel = null;
			}
		}

		public void write(IRecordRef which, IDictionary<string, object> record)
		{
			if (_committed)
				throw new UnrecoverableOperationException($"Attempted to write record {which.RecordName} after commit.");

			if (!(which is RecordHandler))
				throw new UnrecoverableOperationException($"RecordRef for {which.RecordName} is not of type RecordHandler");

			var rh = which as RecordHandler;

			rh.write(_channel, record);
		}
	}
}
