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
using SDK;

namespace OutputBootstrapper
{
	public class Output
	{
		private static readonly ILog _log = LogManager.GetLogger(typeof(Output));
		private static IOutputFactory _outFactory;
		private static IContainer _outContainer;


		private static CxAnalytixService Service => Config.GetConfig<CxAnalytixService>();

		private static Assembly[] GetOutputAssemblies()
        {
			string[] runtimeAssemblies = Directory.GetFiles(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location), "*.dll");
			List<Assembly> retVal = new List<Assembly>();

            foreach (var path in runtimeAssemblies)
            {
				try
				{
					var asm = Assembly.LoadFrom(path);
					retVal.Add(asm);
				}
				catch (BadImageFormatException)
                {
					continue;
                }
            }

            return retVal.ToArray();
        }

		static Output()
		{
			var builder = new ContainerBuilder();
			builder.RegisterAssemblyModules(typeof(IOutputFactory), GetOutputAssemblies());
			_outContainer = builder.Build();

			try
			{
				_outFactory = _outContainer.ResolveNamed<IOutputFactory>(Service.OutputModuleName.ToLower() );
			}
			catch (ComponentNotRegisteredException ex)
            {
				_log.Error($"Output module with name '{Service.OutputModuleName}' not found, name must be one of: [{String.Join(",", Registrar.ModuleRegistry.ModuleNames)}]");
				throw new ProcessFatalException($"Unknown output module '{Service.OutputModuleName}'", ex);
			}
		}

        private static void ComponentRegistryBuilder_Registered(object sender, Autofac.Core.ComponentRegisteredEventArgs e)
        {
            throw new NotImplementedException();
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
