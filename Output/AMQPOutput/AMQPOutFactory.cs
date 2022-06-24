using Autofac;
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

	public class AMQPOutFactory : SDK.OutputModule, IDisposable
	{
		private static ILog _log = LogManager.GetLogger(typeof(AMQPOutFactory));

		private static ConnectionFactory _amqpFactory = new ConnectionFactory();


		private IConnection _connection = null;
		private IConnection Connection { 
			get
			{
				if (_connection == null)
				{
					foreach (var ep in _conCfg.Endpoints as IEnumerable<AmqpTcpEndpoint>)
						_log.Info($"AMQP endpoint: {ep.HostName}:{ep.Port} SSL: {ep.Ssl.Enabled}");

					_connection = _amqpFactory.CreateConnection(_conCfg.Endpoints, Assembly.GetEntryAssembly().GetName().ToString());
				}

				return _connection;
			}
		}

        [Import]
		private IAmqpConnectionConfig _conCfg { get; set; }

		public AMQPOutFactory() : base("AMQP", typeof(AMQPOutFactory))
		{
			CxAnalytix.Configuration.Impls.Config.InjectConfigs(this);

			_amqpFactory.UserName = _conCfg.UserName;
			_amqpFactory.Password = _conCfg.Password;
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
