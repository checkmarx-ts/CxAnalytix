using CxAnalytix.Configuration.Contracts;
using CxAnalytix.Configuration.Impls;
using CxAnalytix.Exceptions;
using CxAnalytix.Extensions;
using CxAnalytix.Interfaces.Outputs;
using log4net;
using System;
using System.Composition;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace OutputBootstrapper
{
	public class Output
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(Output));
		private static IOutputFactory _outFactory;

		[Import]
		private static ICxAnalytixService Service { get; set; }

		static Output()
		{
			Service = Config.GetConfig<ICxAnalytixService>(Assembly.GetExecutingAssembly());
			try
			{
				Assembly outAssembly = Assembly.Load(Service.OutputAssembly);
				_log.DebugFormat("outAssembly loaded: {0}", outAssembly.FullName);
				_outFactory = outAssembly.CreateInstance(Service.OutputClass) as IOutputFactory;

				if (_outFactory == null)
					throw new ProcessFatalException($"Could not load the output factory with the name {Service.OutputClass} in assembly {outAssembly.FullName}");

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
