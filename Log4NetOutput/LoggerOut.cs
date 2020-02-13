using CxAnalytics.TransformLogic;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace CxAnalytics.Out.Log4NetOutput
{
    internal sealed class LoggerOut : IOutput
    {
        private readonly ILog _recordLog = null;
        private static readonly ILog _log = LogManager.GetLogger(typeof (LoggerOut));
        private readonly String _recordType;

        internal LoggerOut (String recordType)
        {
            _recordType = recordType;
            _recordLog = LogManager.Exists(Assembly.GetExecutingAssembly(), recordType);
            _log.DebugFormat("Created LoggerOut with record type {0}", recordType);
        }

        public void write(IDictionary<string, string> record)
        {
            _log.DebugFormat("Logger for record type [{0}] writing record with {1} elements.", _recordType, record.Keys.Count);
            if (_recordLog == null)
            {
                _log.Warn($"Logger for recordType {_recordType} is null, logging is misconfigured.");
                return;
            }

            _recordLog.Info(JsonConvert.SerializeObject (record));
        }
    }
}
