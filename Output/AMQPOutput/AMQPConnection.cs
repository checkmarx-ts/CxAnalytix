using CxAnalytix.Out.AMQPOutput.Config.Impls;
using log4net;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Out.AMQPOutput
{
    internal abstract class AMQPConnection
    {
        private static ILog _log = LogManager.GetLogger(typeof(AMQPConnection));

        private static ConnectionFactory _amqpFactory = new ConnectionFactory();

        private static AmqpConnectionConfig ConConfig => CxAnalytix.Configuration.Impls.Config.GetConfig<AmqpConnectionConfig>();

        static AMQPConnection()
        {
            _amqpFactory.UserName = ConConfig.UserName;
            _amqpFactory.Password = ConConfig.Password;
        }

        private static IConnection _connection = null;
        private static IConnection Connection
        {
            get
            {
                if (_connection == null)
                {
                    _log.Info("Connecting to AMQP endpoints.");

                    foreach (var ep in ConConfig.Endpoints as IEnumerable<AmqpTcpEndpoint>)
                        _log.Debug($"AMQP endpoint: {ep.HostName}:{ep.Port} SSL: {ep.Ssl.Enabled}");

                    var agent = CxAnalytix.Utilities.Reflection.GetUserAgentName();

                    _connection = _amqpFactory.CreateConnection(ConConfig.Endpoints, $"{agent.CompanyName}-{agent.ProductName}-{agent.ProductVersion}");
                }

                return _connection;
            }
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static IModel GetModel()
        {
            if (!Connection.IsOpen)
                ResetConnection();

            return Connection.CreateModel();
        }

        private static void ResetConnection()
        {
            if (_connection != null)
            {
                _connection.Dispose();
                _connection = null;
            }

        }

    }
}
