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
		private HashSet<String> _records;

		public LoggerOutTransaction(IEnumerable<String> recordNames)
		{
			_records = new HashSet<string>(recordNames);
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
			if (!_records.Contains(which.RecordName))
				throw new UnrecoverableOperationException($"Attempting to write to unregistered record {which.RecordName}");

			if (!_loggers.ContainsKey(which.RecordName))
				_loggers.Add(which.RecordName, new LoggerOut(which.RecordName) );

			_loggers[which.RecordName].stage(record);
		}
	}
}
