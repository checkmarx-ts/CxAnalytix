using Autofac;
using Autofac.Core.Registration;
using CxAnalytix.Configuration.Impls;
using CxAnalytix.Exceptions;
using CxAnalytix.Extensions;
using CxAnalytix.Interfaces.Outputs;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using SDK.Modules;
using CxAnalytix.Utilities;

namespace OutputBootstrapper
{
	public class Output
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(Output));
		private static IOutputFactory _outFactory;
		private static IContainer _outContainer;


		private static CxAnalytixService Service => Config.GetConfig<CxAnalytixService>();


		static Output()
		{
			var builder = new ContainerBuilder();
			builder.RegisterAssemblyModules(typeof(IOutputFactory), Reflection.GetOutputAssemblies());
			_outContainer = builder.Build();

			try
			{
				_outFactory = _outContainer.ResolveNamed<IOutputFactory>(Service.OutputModuleName.ToLower() );
			}
			catch (ComponentNotRegisteredException ex)
            {
				String availableModules = String.Join(",", Registrar.ModuleRegistry.GetModuleNames<IOutputFactory>());
				_log.Error($"Output module with name '{Service.OutputModuleName}' not found, name must be one of: [{availableModules}]");
				throw new ProcessFatalException($"Unknown output module '{Service.OutputModuleName}'", ex);
			}
		}

        [MethodImpl(MethodImplOptions.Synchronized)]
		public static IOutputTransaction StartTransaction()
		{
			var retVal =  new RecordSuppressionMediator(_outFactory.StartTransaction());

			_log.Trace($"Starting output transaction: {retVal.TransactionId}");

			return retVal;
		}

		[MethodImpl(MethodImplOptions.Synchronized)]
		public static IRecordRef RegisterRecord(String name)
		{
			if (String.IsNullOrEmpty(name))
				return new RecordRefWrapper();

			_log.Debug($"Registering record with name {name}");

			return new RecordRefWrapper(_outFactory.RegisterRecord(name));
		}

	}
}
