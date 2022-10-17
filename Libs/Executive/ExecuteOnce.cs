using System;
using System.Reflection;
using log4net;
using CxAnalytix.Configuration.Impls;
using CxAnalytix.AuditTrails.Crawler;
using OutputBootstrapper;
using Autofac;
using CxAnalytix.Utilities;
using CxAnalytix.Interfaces.Transform;
using Autofac.Core.Registration;
using SDK.Modules;
using CxAnalytix.Exceptions;
using CxAnalytix.Configuration.Utils;

[assembly: CxRestClient.IO.NetworkTraceLog()]
[assembly: CxAnalytix.Extensions.LogTrace()]


namespace CxAnalytix.Executive
{



    public class ExecuteOnce
    {
        private static readonly String LOG_CONFIG_FILE_NAME = "cxanalytix.log4net";

        private static IContainer _xformersContainer;

        protected static CxAnalytixService Service => Config.GetConfig<CxAnalytixService>();

        private static CancellationTokenSource _defaultCancelToken = new CancellationTokenSource();

        private static readonly ILog _log = LogManager.GetLogger(typeof(ExecuteOnce));


        static ExecuteOnce()
        {
            var l4net_config = new FileInfo(ConfigPathResolver.ResolveConfigFilePath (LOG_CONFIG_FILE_NAME) );
            log4net.Config.XmlConfigurator.Configure(l4net_config);

            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(typeof(ITransformer), Reflection.GetOutputAssemblies());
            _xformersContainer = builder.Build();
        }

        private static IEnumerable<ITransformer> LoadTransformers()
        {
            var retVal = new LinkedList<ITransformer>();
            foreach (EnabledTransformer requestedXform in Service.Transformers)
            {
                try
                {
                    retVal.AddLast(_xformersContainer.ResolveNamed<ITransformer>(requestedXform.Name.ToLower()));
                }
                catch (ComponentNotRegisteredException ex)
                {
                    String availableModules = String.Join(",", Registrar.ModuleRegistry.GetModuleNames<ITransformer>());
                    _log.Error($"Transformer with name '{requestedXform.Name}' not found, name must be one of: [{availableModules}]");
                    throw new ProcessFatalException($"Unknown transformer module '{requestedXform.Name}'", ex);
                }
            }

            return retVal;
        }

        public static void Execute(CancellationTokenSource? t = null)
        {
            if (t == null)
                t = _defaultCancelToken;

            var entry = Assembly.GetEntryAssembly();


            _log.Info($"Start via [{(entry != null ? entry.GetName() : "UNKNOWN")}]");

            _log.InfoFormat("CWD: {0}", Directory.GetCurrentDirectory());

            DateTime start = DateTime.Now;
            _log.Info($"Starting data transformation with {Service.Transformers.Count} transformers.");

            Parallel.ForEach<ITransformer>(LoadTransformers(), new ParallelOptions() { CancellationToken = t.Token
            }, 
            (xformer) => {

                _log.Info($"Begin executing data transformer for module: {xformer.DisplayName}");
                try
                {
                    xformer.DoTransform(t.Token);
                }
                catch (Exception ex)
                {
                    _log.Error($"Unhandled exception when executing transformer module: {xformer.DisplayName}", ex);

                }

                _log.Info($"Finished executing data transformer for module: {xformer.DisplayName}");

                xformer.Dispose();

            });

            _log.InfoFormat("Data transformation finished in {0:0.00} minutes.",
                DateTime.Now.Subtract(start).TotalMinutes);

            start = DateTime.Now;

            if (!t.Token.IsCancellationRequested)
                using (var auditTrx = Output.StartTransaction())
                {
                    AuditTrailCrawler.CrawlAuditTrails(t.Token);

                    if (!t.Token.IsCancellationRequested)
                        auditTrx.Commit();
                }

            _log.InfoFormat("Audit data transformation finished in {0:0.00} minutes.",
                DateTime.Now.Subtract(start).TotalMinutes);


            _log.Info("End");
        }
    }
}