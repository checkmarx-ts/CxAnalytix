using CxAnalytix.Exceptions;
using CxAnalytix.Interfaces.Outputs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.Log4NetOutput
{
	internal class LoggerOutTransaction : IOutputTransaction
	{

		private Dictionary<String, LoggerOut> _loggers = new Dictionary<string, LoggerOut>();

		public LoggerOutTransaction(IEnumerable<String> recordNames)
		{
			foreach (var rec in recordNames)
				_loggers.Add(rec, new LoggerOut(rec) );
		}

		public bool Commit()
		{
			foreach (var logger in _loggers.Values)
				logger.commit();

			return true;
		}

		public void Dispose()
		{
			foreach (var logger in _loggers.Values)
				logger.Dispose();
		}

		public void write(IRecordRef which, IDictionary<string, object> record)
		{
			if (!_loggers.ContainsKey(which.RecordName))
				throw new UnrecoverableOperationException($"Attempting to write to unregistered record {which.RecordName}");

			_loggers[which.RecordName].stage(record);
		}
	}
}
