using CxAnalytix.Exceptions;
using CxAnalytix.Interfaces.Outputs;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Out.Log4NetOutput
{
	internal class LoggerOutTransaction<T> : IOutputTransaction where T : LoggerOut
	{

		private Dictionary<String, T> _loggers = new Dictionary<string, T>();
		private HashSet<String> _records;
		private Guid _id = Guid.NewGuid ();

		public LoggerOutTransaction(IEnumerable<String> recordNames)
		{
			_records = new HashSet<string>(recordNames);
		}

		public string TransactionId => _id.ToString();

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
				_loggers.Add(which.RecordName, (T)Activator.CreateInstance(typeof(T), new object[] { which.RecordName } ) );

			_loggers[which.RecordName].stage(record);
		}
	}
}
