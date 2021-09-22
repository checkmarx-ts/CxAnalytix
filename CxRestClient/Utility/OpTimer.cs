using log4net;
using System;
using CxAnalytix.Extensions;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient.Utility
{
	internal class OpTimer : IDisposable
	{

		private static ILog _log = LogManager.GetLogger(typeof(OpTimer));

		private DateTime _start;
		private String _op;

		internal OpTimer(String operationName)
		{
			_start = DateTime.Now;
			_op = operationName;

		}

		public void Dispose()
		{
			if (_log.Logger.IsEnabledFor(log4net.Core.Level.Trace))
			{
				_log.Trace($"Operation [{_op}] completed in [{DateTime.Now.Subtract(_start).TotalMilliseconds:0.##}ms]");
			}
		}
	}
}
