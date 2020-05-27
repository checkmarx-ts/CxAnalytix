using CxAnalytix.Configuration;
using CxAnalytix.TransformLogic;
using log4net;
using LogCleaner;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace CxAnalytix.Out.Log4NetOutput
{
    internal sealed class LoggerOut : IOutput
    {
        private readonly ILog _recordLog = null;
        private static readonly ILog _log = LogManager.GetLogger(typeof (LoggerOut));
        private readonly String _recordType;
        private static readonly String CONFIG_SECTION = "CxLogOutput";
        private static CancellationTokenSource _token;
        private static Task _task = null;

        private static readonly String DATE_FORMAT = "yyyy-MM-ddTHH:mm:ss.fffzzz";

        private JsonSerializerSettings _serSettings = new JsonSerializerSettings()
        {
            DateFormatString = DATE_FORMAT,
            NullValueHandling = NullValueHandling.Ignore,
            StringEscapeHandling = StringEscapeHandling.EscapeHtml,
            Converters = new List<JsonConverter>()
            {
                new PrimitiveJsonConverter ()
            }

        };


        static LoggerOut ()
        {
            _token = new CancellationTokenSource();
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;


            _task = Task.Run(async () => 
            {
                var cfg = Config.GetConfig<LogOutputConfig>(CONFIG_SECTION);

                while (!_token.IsCancellationRequested)
                {
                    foreach (FileSpecElement spec in cfg.PurgeSpecs)
                    {
                        Cleaner.CleanOldFiles(cfg.OutputRoot, spec.MatchSpec, cfg.DataRetentionDays);
                    }
                    await Task.Delay(60000 * 60, _token.Token);

                    _token.Token.ThrowIfCancellationRequested();
                }
            }, _token.Token);
        }

        private static void OnProcessExit(object sender, EventArgs e)
        {
            _token.Cancel();

            try
            {
                _task.Wait();
            }
            catch (AggregateException)
            {
            }
        }

        internal LoggerOut (String recordType)
        {
            _recordType = recordType;
            _recordLog = LogManager.Exists(Assembly.GetExecutingAssembly(), recordType);
            _log.DebugFormat("Created LoggerOut with record type {0}", recordType);
        }

        public void write(IDictionary<string, object> record)
        {
            _log.DebugFormat("Logger for record type [{0}] writing record with {1} elements.", _recordType, record.Keys.Count);
            if (_recordLog == null)
            {
                _log.Warn($"Logger for recordType {_recordType} is null, logging is misconfigured.");
                return;
            }

            _recordLog.Info(JsonConvert.SerializeObject (record, _serSettings));
        }
    }
}
