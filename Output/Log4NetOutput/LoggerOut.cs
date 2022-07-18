using CxAnalytix.Configuration;
using CxAnalytix.Exceptions;
using CxAnalytix.Out.Log4NetOutput.Config.Contracts;
using CxAnalytix.Out.Log4NetOutput.Config.Impl;
using CxAnalytix.Utilities.Json;
using log4net;
using LogCleaner;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxAnalytix.Out.Log4NetOutput
{
	internal class LoggerOut : IDisposable
	{
        private static readonly ILog _log = LogManager.GetLogger(typeof(LoggerOut));

        private static CancellationTokenSource _token;
        private static Task _task = null;
        protected readonly String _recordType;
        protected readonly ILog _recordLog = null;

        private static LogOutputConfig OutConfig => CxAnalytix.Configuration.Impls.Config.GetConfig<LogOutputConfig>();

        public LoggerOut(String recordType)
        {
            _recordType = recordType;
            _recordLog = LogManager.Exists(Assembly.GetExecutingAssembly(), recordType);
            if (_recordLog == null)
                throw new ProcessFatalException($"Logger for recordType {recordType} was not created. " +
                    $"The log4net configuration is not correct.");

        }

        static LoggerOut()
        {

            _token = new CancellationTokenSource();
            AppDomain.CurrentDomain.ProcessExit += OnProcessExit;


            _task = Task.Run(async () =>
            {

                while (!_token.IsCancellationRequested)
                {
                    foreach (FileSpecElement spec in OutConfig.PurgeSpecs)
                    {
                        Cleaner.CleanOldFiles(OutConfig.OutputRoot, spec.MatchSpec, OutConfig.DataRetentionDays);
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

        protected String SerializeDictionary(IDictionary<string, object> record)
		{
            return JsonConvert.SerializeObject(record, Defs.serializerSettings);
        }

        public virtual void stage(IDictionary<string, object> record)
		{
            _recordLog.Info(SerializeDictionary(record));
        }

        public virtual void commit()
		{

		}

        public virtual void Dispose()
        { }
    }
}
