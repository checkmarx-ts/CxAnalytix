using CxAnalytix.Configuration;
using CxAnalytix.Exceptions;
using CxAnalytix.Extensions;
using CxAnalytix.Interfaces.Outputs;
using log4net;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OutputBootstrapper
{
	public class Output
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(Output));
		private static IOutputFactory _outFactory;

		static Output()
		{
			try
			{
				Assembly outAssembly = Assembly.Load(Config.Service.OutputAssembly);
				_log.DebugFormat("outAssembly loaded: {0}", outAssembly.FullName);
				_outFactory = outAssembly.CreateInstance(Config.Service.OutputClass) as IOutputFactory;

				if (_outFactory == null)
					throw new ProcessFatalException($"Could not load the output factory with the name {Config.Service.OutputClass} in assembly {outAssembly.FullName}");

				_log.Debug("IOutputFactory instance created.");
			}
			catch (Exception ex)
			{
				throw new ProcessFatalException("Error loading output factory.", ex);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]

		public static IOutputTransaction StartTransaction()
		{
			var retVal =  _outFactory.StartTransaction();

			_log.Trace($"Starting output transaction: {retVal.TransactionId}");

			return retVal;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static IRecordRef RegisterRecord(String name)
		{
			_log.Debug($"Registering record with name {name}");

			return _outFactory.RegisterRecord(name);
		}

	}
}
