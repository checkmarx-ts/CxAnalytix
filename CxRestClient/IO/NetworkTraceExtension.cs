using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using log4net;
using log4net.Repository;


namespace CxRestClient.IO
{

    internal static class NetworkTraceExtension
    {
        private static readonly String TRACE_NAME = "TRACE_NETWORK";
        private static readonly String TRACE_DISPLAY = "NETWORK";
        internal static log4net.Core.Level _netTraceLevel = 
            new log4net.Core.Level(19999, TRACE_NAME, TRACE_DISPLAY);

        public static bool IsNetworkTrace (this ILog log)
        {
            return log.Logger.IsEnabledFor(_netTraceLevel);
        }

        public static void NetworkTrace(this ILog log, string message)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                _netTraceLevel, message, null);
        }

        public static void NetworkTraceFormat(this ILog log, string spec, params object[] args)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                _netTraceLevel, String.Format(spec, args), null);
        }

        public static void NetworkTrace(this ILog log, string message, Exception ex)
        {
            log.Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType,
                _netTraceLevel, message, null);
        }

    }
}
