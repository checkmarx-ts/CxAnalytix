using System;
using System.Collections.Generic;
using CxAnalytix.Configuration.Impls;
using CxAnalytix.Interfaces.Outputs;

namespace CxAnalytix.Out.Log4NetOutput
{
    public sealed class LoggerOutFactory : SDK.Modules.OutputModule
	{
		private HashSet<String> _recs = new HashSet<String>();

		private CxAnalytixService Service => CxAnalytix.Configuration.Impls.Config.GetConfig<CxAnalytixService>();

		public LoggerOutFactory() : base("Log4Net", typeof(LoggerOutFactory) )
        {
		}

		internal class Ref : IRecordRef
		{
			public String RecordName { get; internal set; }
		};

		public override IRecordRef RegisterRecord(string recordName)
		{
			if (String.IsNullOrEmpty(recordName))
				throw new InvalidOperationException("Creating a logger with a blank record type is not valid.");

			_recs.Add(recordName);
			return new Ref() { RecordName = recordName };
		}

		public override IOutputTransaction StartTransaction()
		{
			if (Service.EnablePseudoTransactions)
				return new LoggerOutTransaction<TransactionalLoggerOut>(_recs);
			else
				return new LoggerOutTransaction<LoggerOut>(_recs);
		}

    }
}
