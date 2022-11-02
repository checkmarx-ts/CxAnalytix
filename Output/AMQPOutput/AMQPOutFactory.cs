using Autofac;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.Out.AMQPOutput.Config.Impls;
using log4net;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CxAnalytix.Out.AMQPOutput
{

	public class AMQPOutFactory : SDK.Modules.Output.OutputModule
	{
		private static ILog _log = LogManager.GetLogger(typeof(AMQPOutFactory));



		public AMQPOutFactory() : base("AMQP", typeof(AMQPOutFactory))
		{

		}

		public override IRecordRef RegisterRecord(string recordName)
		{
			return new RecordHandler(recordName);
		}


		public override IOutputTransaction StartTransaction() => new TransactionHandler();

	}
}
