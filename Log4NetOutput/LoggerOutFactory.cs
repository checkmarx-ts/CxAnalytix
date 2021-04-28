using System;
using System.Collections.Generic;
using CxAnalytix.Exceptions;
using CxAnalytix.Interfaces.Outputs;

namespace CxAnalytix.Out.Log4NetOutput
{
    public sealed class LoggerOutFactory : IOutputFactory
    {
		private HashSet<String> _recs = new HashSet<String>();

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
			return new LoggerOutTransaction(_recs);
		}
	}
}
