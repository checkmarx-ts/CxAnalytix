using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Out.AMQPOutput.Config.Contracts;
using log4net;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;

namespace CxAnalytix.Out.AMQPOutput
{

	public class AMQPOutFactory : IOutputFactory, IDisposable
	{
		private static ILog _log = LogManager.GetLogger(typeof(AMQPOutFactory));

		private static ConnectionFactory _amqpFactory = new ConnectionFactory();
		private IConnection _connection;

        [Import]
		private IAmqpConnectionConfig _conCfg { get; set; }

		public AMQPOutFactory()
		{
			CxAnalytix.Configuration.Impls.Config.InjectMyConfigs(this, Assembly.GetExecutingAssembly());

			_amqpFactory.UserName = _conCfg.UserName;
			_amqpFactory.Password = _conCfg.Password;

			var endpoints = _conCfg.Endpoints;

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
