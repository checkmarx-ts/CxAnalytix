using log4net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using CxAnalytics.TransformLogic;
using CxAnalytics.Configuration;
using System.Reflection;
using System;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "CxAnalyticsExportService.log4net", Watch = true)]

namespace CxAnalyticsExportService
{
    class ServiceLifecycleControl : ServiceBase
    {
        private static ILog _log = LogManager.GetLogger(typeof(ServiceLifecycleControl));
        private static IOutputFactory _outFactory = null;

        static ServiceLifecycleControl()
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

        public ServiceLifecycleControl ()
        {
            CanHandlePowerEvent = false;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;
            CanHandlePowerEvent = true;
        }

        private void stopService ()
        {
            if (_cancelToken != null)
            {
                _cancelToken.Cancel();

                if (_serviceTask != null)
                {
                    _log.Debug("Waiting for the service task to complete after cancellation.");

                    try
                    {
                        _serviceTask.Wait();
                    }
                    catch (AggregateException ex)
                    {
                        _log.Debug("Task finished normally and exception has been logged.", ex);
                    }

                    _log.Debug("Service task has stopped after wait.");
                }
                else
                    _log.Warn("Task was null when the service was stopped.");

                _serviceTask.Dispose();
                _serviceTask = null;
                _cancelToken.Dispose();
                _cancelToken = null;
            }
            else
                _log.Warn("Cancellation token was null when service was stopped.");

        }

        protected override void OnShutdown()
        {
            _log.Info("Service is stopping due to shutdown.");
            stopService();
            base.OnShutdown();
        }

        private CancellationTokenSource _cancelToken = null;
        private Task _serviceTask = null;

        protected override void OnStart(string[] args)
        {
            base.OnStart(args);

            _log.Info("Service start.");

            _cancelToken = new CancellationTokenSource();

            _serviceTask = Task.Run(async () =>
            {
                do
                {
                    DateTime start = DateTime.Now;
                    _log.Info("Starting data transformation.");
                    Transformer.doTransform(Config.Service.ConcurrentThreads, _outFactory, _cancelToken.Token);
                    _log.InfoFormat("Data transformation finished in {0:0.00} minutes.", DateTime.Now.Subtract(start).TotalMinutes);
                    await Task.Delay(Config.Service.ProcessPeriodMinutes * 60 * 1000, _cancelToken.Token);
                } while (!_cancelToken.Token.IsCancellationRequested);

                _cancelToken.Token.ThrowIfCancellationRequested();

            }, _cancelToken.Token);

        }

        protected override void OnStop()
        {
            _log.Info("Service is stopping due to stop request.");
            stopService();
            base.OnStop();
        }
    }
}
