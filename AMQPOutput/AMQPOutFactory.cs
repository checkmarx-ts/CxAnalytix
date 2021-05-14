using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Out.AMQPOutput.Config;
using log4net;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.AMQPOutput
{

	/**
	 * Need to add field filters for record types so that certain fields can be removed to keep message size small.  Cloud message
	 * services limit the size of the messages.  Make it so there is a pass filter which limits the message to only certain fields
	 * or a reject filter that rejects specified fields.
	 *  
	 * Add the ability to set topics or headers composed of values from the dictionary per record.
	 * 
 	 * Need to allow paramters like "x-match" to be sent as part of the binding to the exchange
	 * 
	 * Make sue URI spec will work with at least AWS cloud based AMQP endpoints
	 * 
	 * How about optional exchange override per record type?  Global exchange is defined once and is required (can be blank), but
	 * each record type can go to a specific exchange.
	 * 
	 *  
	 */

	public class AMQPOutFactory : IOutputFactory, IDisposable
	{
		private static ILog _log = LogManager.GetLogger(typeof(AMQPOutFactory));

		private static ConnectionFactory _amqpFactory = new ConnectionFactory();
		private static IConnection _connection;

		static AMQPOutFactory()
		{
			var endpoints = Configuration.Config.GetConfig<AmqpConnectionConfig> (AmqpConnectionConfig.SECTION_NAME).Endpoints;

			foreach (var ep in endpoints as IEnumerable<AmqpTcpEndpoint>)
				_log.Info($"AMQP endpoint: {ep.HostName}:{ep.Port} SSL: {ep.Ssl.Enabled}");

			_connection = _amqpFactory.CreateConnection(endpoints);
		}


	public IRecordRef RegisterRecord(string recordName)
		{
			// TODO: RecordHandler should have the implementation for the specific
			// call to perform the publish....it may need to produce another class that holds the connection.
			// it should know which exchange, topic, and headers to send.  it should also know how to form the json message with 
			// the correct fields filtered for that record.

			return new RecordHandler(recordName);
		}

		public IOutputTransaction StartTransaction()
		{

			return new TransactionHandler(_connection.CreateModel() );

			// Start transaction should make a channel and do the publish to a specific exchange with the correct routing key
			// and then shut down the connection
			//var channel = _connection.CreateModel();

			//var q = channel.QueueDeclare(recordName, exclusive: false, autoDelete: false);

			//channel.BasicPublish("", recordName, null, Encoding.UTF8.GetBytes("HELLO"));
			//throw new NotImplementedException();
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
