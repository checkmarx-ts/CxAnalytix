using CxAnalytix.Configuration;
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
				_log.Debug("IOutputFactory instance created.");
			}
			catch (Exception ex)
			{
				_log.Error("Error loading output factory.", ex);
			}
		}

		[MethodImpl(MethodImplOptions.Synchronized)]

		public static IOutputTransaction StartTransaction()
		{
			_log.Debug("Starting output transaction");

			return _outFactory.StartTransaction();
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static IRecordRef RegisterRecord(String name)
		{
			_log.Debug($"Registering record with name {name}");

			return _outFactory.RegisterRecord(name);
		}

	}
}
