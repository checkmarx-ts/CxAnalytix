using log4net;
using System;
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
				_log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, log4net.Core.Level.Trace, 
					$"Operation [{_op}] completed in [{DateTime.Now.Subtract(_start).TotalMilliseconds:0.##}ms]", null);
			}
		}
	}
}
