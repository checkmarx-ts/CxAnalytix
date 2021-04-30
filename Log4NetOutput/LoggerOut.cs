using CxAnalytix.Configuration;
using log4net;
using LogCleaner;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CxAnalytix.Exceptions;
using System.IO;
using CxAnalytix.Utilities;
using System.Runtime.CompilerServices;
using CxAnalytix.Extensions;

namespace CxAnalytix.Out.Log4NetOutput
{
    internal sealed class LoggerOut : IDisposable
    {
        private readonly ILog _recordLog = null;
        private static readonly ILog _log = LogManager.GetLogger(typeof (LoggerOut));
        private readonly String _recordType;
        private static readonly String CONFIG_SECTION = "CxLogOutput";
        private static CancellationTokenSource _token;
        private static Task _task = null;
        private bool _committed = false;
        private TextWriter _stage;
        private SharedMemoryStream _stageStorage;

        private Object _sync = new object();

        private static long INITIAL_CAPACITY = 2048000;

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
            if (_recordLog == null)
                throw new ProcessFatalException($"Logger for recordType {recordType} was not created. " +
                    $"The log4net configuration is not correct.");


            _stageStorage = new SharedMemoryStream(INITIAL_CAPACITY);
            _stage = new StreamWriter(_stageStorage, leaveOpen: true);

            _log.DebugFormat("Created LoggerOut with record type {0}", recordType);
        }


        [MethodImpl(MethodImplOptions.Synchronized)]

        public void stage(IDictionary<string, object> record)
        {
            if (_committed)
                throw new UnrecoverableOperationException
                    ($"{_recordType}: Attempted to stage a record after the transaction has been committed.");

            if (record == null || record.Count == 0)
                return;

            _log.TraceFormat("Logger for record type [{0}] staging record with {1} elements.", _recordType, record.Keys.Count);

            var obj = JsonConvert.SerializeObject(record, _serSettings);

            _stage.WriteLine(obj);
		}

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void commit ()
		{
            if (_committed)
                throw new UnrecoverableOperationException
                    ($"{_recordType}: Attempted to commit a transaction that has already been committed.");

            _committed = true;

            _stage.Flush();
            _stage.Dispose();
            _stage = null;

            if (_stageStorage.Seek(0, SeekOrigin.Begin) != 0)
                throw new UnrecoverableOperationException($"{_recordType}: could not seek to beginning of storage.");


            var timer = DateTime.Now;
            long recordCount = 0;

            using (var reader = new StreamReader(_stageStorage))
                while (true)
                {
                    var line = reader.ReadLine();
                    if (line == null)
                        break;
                    else
                    {
                        _recordLog.Info(line);
                        recordCount++;
                    }
                }
            
            _log.Debug($"COMMITTED: {recordCount} records for {_recordType} in {DateTime.Now.Subtract(timer).TotalMilliseconds}ms");
        }

        public void Dispose()
		{
            lock (_sync)
			{
                if (_stage != null)
                    _stage.Dispose();

                if (_stageStorage != null)
                    _stageStorage.Dispose();

			}
		}
	}
}
