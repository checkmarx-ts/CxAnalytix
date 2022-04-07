using log4net;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using CxAnalytix.TransformLogic;
using CxAnalytix.Configuration;
using System;
using CxRestClient;
using CxAnalytix.AuditTrails.Crawler;
using CxAnalytix.Exceptions;
using ProjectFilter;
using CxRestClient.Utility;

[assembly: CxRestClient.IO.NetworkTraceLog()]
[assembly: CxAnalytix.Extensions.LogTrace()]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "CxAnalytixService.log4net", Watch = true)]

namespace CxAnalytixService
{
    class ServiceLifecycleControl : ServiceBase
    {
        private static ILog _log = LogManager.GetLogger(typeof(ServiceLifecycleControl));


        public ServiceLifecycleControl()
        {
            CanHandlePowerEvent = false;
            CanPauseAndContinue = false;
            CanShutdown = true;
            CanStop = true;
        }

        private void stopService()
        {
            if (_cancelToken != null)
            {
                _cancelToken.Cancel();

                if (_serviceTask != null && !_serviceTask.IsCompleted)
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


            var builder = new CxSASTRestContext.CxSASTRestContextBuilder();
            builder.WithSASTServiceURL(Config.Connection.URL).
            WithMNOServiceURL(Config.Connection.MNOUrl)
            .WithOpTimeout(Config.Connection.TimeoutSeconds)
            .WithSSLValidate(Config.Connection.ValidateCertificates)
            .WithUsername(Config.Credentials.Username)
            .WithPassword(Config.Credentials.Password)
            .WithRetryLoop(Config.Connection.RetryLoop);

            var restCtx = builder.Build();

			_serviceTask = Task.Run(async () =>
            {
                do
                {
                    DateTime start = DateTime.Now;
                    _log.Info("Starting data transformation.");

                    try
                    {
						Transformer.DoTransform(Config.Service.ConcurrentThreads,
						Config.Service.StateDataStoragePath, Config.Service.InstanceIdentifier,
						restCtx,
						new FilterImpl(Config.GetConfig<CxFilter>("ProjectFilterRegex").TeamRegex,
						Config.GetConfig<CxFilter>("ProjectFilterRegex").ProjectRegex),
						new RecordNames()
						{
							SASTScanSummary = Config.Service.SASTScanSummaryRecordName,
							SASTScanDetail = Config.Service.SASTScanDetailRecordName,
							SCAScanSummary = Config.Service.SCAScanSummaryRecordName,
							SCAScanDetail = Config.Service.SCAScanDetailRecordName,
							ProjectInfo = Config.Service.ProjectInfoRecordName,
							PolicyViolations = Config.Service.PolicyViolationsRecordName
						}, _cancelToken.Token, !String.IsNullOrEmpty(Config.Connection.MNOUrl), 
                        LicenseChecks.OsaIsLicensed(restCtx, _cancelToken.Token));

					}
					catch (ProcessFatalException pfe)
					{
						Fatal(pfe);
						break;
					}
					catch (TypeInitializationException ex)
                    {
                        Fatal(ex);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Vulnerability data transformation aborted due to unhandled exception.", ex);
                    }

                    _log.InfoFormat("Vulnerability data transformation finished in {0:0.00} minutes.",
                        DateTime.Now.Subtract(start).TotalMinutes);

                    start = DateTime.Now;

                    try
                    {
                        if (!_cancelToken.Token.IsCancellationRequested)
							AuditTrailCrawler.CrawlAuditTrails(_cancelToken.Token);
                    }
                    catch (ProcessFatalException pfe)
                    {
                        _log.Error("Fatal exception caught, program ending.", pfe);
                        new Task(() => Stop()).Start();
                        break;
                    }
                    catch (Exception ex)
                    {
                        _log.Error("Audit data transformation aborted due to unhandled exception.", ex);
                    }

                    _log.InfoFormat("Audit data transformation finished in {0:0.00} minutes.",
                        DateTime.Now.Subtract(start).TotalMinutes);

                    await Task.Delay(Config.Service.ProcessPeriodMinutes * 60 * 1000, _cancelToken.Token);
                } while (!_cancelToken.Token.IsCancellationRequested);

                _cancelToken.Token.ThrowIfCancellationRequested();

            }, _cancelToken.Token);

        }

		private void Fatal(Exception ex)
		{
			_log.Error("Fatal exception caught, program ending.", ex);
			new Task(() => Stop()).Start();
		}

		protected override void OnStop()
        {
            _log.Info("Service is stopping due to stop request.");
            stopService();
            base.OnStop();
        }
    }
}
