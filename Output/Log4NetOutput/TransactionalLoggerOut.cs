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
using CxAnalytix.Utilities.Json;

namespace CxAnalytix.Out.Log4NetOutput
{
    internal sealed class TransactionalLoggerOut : LoggerOut
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof (TransactionalLoggerOut));
        private bool _committed = false;
        private TextWriter _stage;
        private SharedMemoryStream _stageStorage;

        private Object _sync = new object();

        private static long INITIAL_CAPACITY = 2048000;


        public TransactionalLoggerOut (String recordType) : base(recordType)
        {
            _stageStorage = new SharedMemoryStream(INITIAL_CAPACITY);
            _stage = new StreamWriter(_stageStorage, leaveOpen: true);

            _log.DebugFormat("Created LoggerOut with record type {0}", recordType);
        }


        [MethodImpl(MethodImplOptions.Synchronized)]

        public override void stage(IDictionary<string, object> record)
        {
            if (_committed)
                throw new UnrecoverableOperationException
                    ($"{_recordType}: Attempted to stage a record after the transaction has been committed.");

            if (record == null || record.Count == 0)
                return;

            _log.TraceFormat("Logger for record type [{0}] staging record with {1} elements.", _recordType, record.Keys.Count);

            _stage.WriteLine(SerializeDictionary(record));
		}

        [MethodImpl(MethodImplOptions.Synchronized)]
        public override void commit ()
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

        public override void Dispose()
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
