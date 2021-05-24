using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Out.AMQPOutput.Config;
using log4net;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput
{

	public class AMQPOutFactory : IOutputFactory, IDisposable
	{
		private static ILog _log = LogManager.GetLogger(typeof(AMQPOutFactory));

		private static ConnectionFactory _amqpFactory = new ConnectionFactory();
		private IConnection _connection;

		public AMQPOutFactory()
		{
			var connection = Configuration.Config.GetConfig<AmqpConnectionConfig>(AmqpConnectionConfig.SECTION_NAME);
			_amqpFactory.UserName = connection.UserName;
			_amqpFactory.Password = connection.Password;

			var endpoints = connection.Endpoints;

			foreach (var ep in endpoints as IEnumerable<AmqpTcpEndpoint>)
				_log.Info($"AMQP endpoint: {ep.HostName}:{ep.Port} SSL: {ep.Ssl.Enabled}");

			_connection = _amqpFactory.CreateConnection(endpoints, "CxAnalytix");
		}


	public IRecordRef RegisterRecord(string recordName)
		{
			return new RecordHandler(recordName);
		}

		public IOutputTransaction StartTransaction()
		{
			return new TransactionHandler(_connection.CreateModel() );
		}

		public void Dispose()
		{
			if (_connection != null)
			{
				_connection.Dispose();
				_connection = null;
			}
		}
	}
}
