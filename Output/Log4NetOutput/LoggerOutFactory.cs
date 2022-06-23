using System;
using System.Collections.Generic;
using System.Composition;
using System.Reflection;
using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Exceptions;
using CxAnalytix.Interfaces.Outputs;

namespace CxAnalytix.Out.Log4NetOutput
{
    public sealed class LoggerOutFactory : IOutputFactory
    {
		private HashSet<String> _recs = new HashSet<String>();


        [Import]
		private ICxAnalytixService Service { get; set; }

		public LoggerOutFactory()
        {
			CxAnalytix.Configuration.Impls.Config.InjectConfigs(this);
		}

		internal class Ref : IRecordRef
		{
			public String RecordName { get; internal set; }
		};

		public IRecordRef RegisterRecord(string recordName)
		{
			if (String.IsNullOrEmpty(recordName))
				throw new InvalidOperationException("Creating a logger with a blank record type is not valid.");

			_recs.Add(recordName);
			return new Ref() { RecordName = recordName };
		}

		public IOutputTransaction StartTransaction()
		{
			if (Service.EnablePseudoTransactions)
				return new LoggerOutTransaction<TransactionalLoggerOut>(_recs);
			else
				return new LoggerOutTransaction<LoggerOut>(_recs);
		}

	}
}
