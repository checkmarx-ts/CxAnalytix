using Autofac;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Out.AMQPOutput.Config.Impls;
using log4net;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CxAnalytix.Out.AMQPOutput
{

	public class AMQPOutFactory : SDK.Modules.Output.OutputModule, IDisposable
	{
		private static ILog _log = LogManager.GetLogger(typeof(AMQPOutFactory));

		private static ConnectionFactory _amqpFactory = new ConnectionFactory();


		private IConnection _connection = null;
		private IConnection Connection { 
			get
			{
				if (_connection == null)
				{
					foreach (var ep in ConConfig.Endpoints as IEnumerable<AmqpTcpEndpoint>)
						_log.Info($"AMQP endpoint: {ep.HostName}:{ep.Port} SSL: {ep.Ssl.Enabled}");

					_connection = _amqpFactory.CreateConnection(ConConfig.Endpoints, Assembly.GetEntryAssembly().GetName().ToString());
				}

				return _connection;
			}
		}

		private AmqpConnectionConfig ConConfig => CxAnalytix.Configuration.Impls.Config.GetConfig<AmqpConnectionConfig>();

		public AMQPOutFactory() : base("AMQP", typeof(AMQPOutFactory))
		{

			_amqpFactory.UserName = ConConfig.UserName;
			_amqpFactory.Password = ConConfig.Password;
		}

        public override IRecordRef RegisterRecord(string recordName)
		{
			return new RecordHandler(recordName);
		}

		public override IOutputTransaction StartTransaction()
		{
			return new TransactionHandler(Connection.CreateModel() );
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
