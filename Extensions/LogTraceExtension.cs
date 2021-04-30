using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Extensions
{
	public static class LogTraceExtension
	{

        private static readonly String TRACE_NAME = "TRACE";
        private static readonly String TRACE_DISPLAY = "TRACE";
        internal static log4net.Core.Level _level =
            new log4net.Core.Level(20000, TRACE_NAME, TRACE_DISPLAY);

        public static bool IsTraceEnabled(this ILog log)
        {
            return log.Logger.IsEnabledFor(_level);
        }

        public static void Trace(this ILog log, string message)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                _level, message, null);
        }

        public static void TraceFormat(this ILog log, string spec, params object[] args)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                _level, String.Format(spec, args), null);
        }

        public static void Trace(this ILog log, string message, Exception ex)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                _level, message, null);
        }

    }
}
