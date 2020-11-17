using CxAnalytix.Configuration;
using CxAnalytix.TransformLogic;
using CxRestClient;
using log4net;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using CxAnalytix.Interfaces.Outputs;
using CxAnalytix.AuditTrails.Crawler;

namespace CxAnalytixDaemon
{
    class Daemon : IHostedService, IDisposable
    {
        private static ILog _log = LogManager.GetLogger(typeof(Daemon));
        private Task _serviceTask;
        private CancellationTokenSource _cancelToken;
        private static IOutputFactory _outFactory = null;

        static Daemon()
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
                _log.Error($"Error loading output factory [{Config.Service.OutputAssembly}].", ex);
            }
        }


        public void Dispose()
        {
            if (_serviceTask != null)
            {
                _serviceTask.Dispose();
                _serviceTask = null;
            }
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _log.Info("Daemon start.");

            _cancelToken = new CancellationTokenSource();

            var builder = new CxRestContext.CxRestContextBuilder();
            builder.WithSASTServiceURL(Config.Connection.URL).
            WithMNOServiceURL(Config.Connection.MNOUrl)
            .WithOpTimeout(Config.Connection.TimeoutSeconds)
            .WithSSLValidate(Config.Connection.ValidateCertificates)
            .WithUsername(Config.Credentials.Username)
            .WithPassword(Config.Credentials.Password);

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
                            restCtx, _outFactory, new RecordNames()
                            {
                                SASTScanSummary = Config.Service.SASTScanSummaryRecordName,
                                SASTScanDetail = Config.Service.SASTScanDetailRecordName,
                                SCAScanSummary = Config.Service.SCAScanSummaryRecordName,
                                SCAScanDetail = Config.Service.SCAScanDetailRecordName,
                                ProjectInfo = Config.Service.ProjectInfoRecordName,
                                PolicyViolations = Config.Service.PolicyViolationsRecordName
                            }, _cancelToken.Token);


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
                            AuditTrailCrawler.CrawlAuditTrails(_outFactory, _cancelToken.Token);
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

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_cancelToken != null && _serviceTask != null && !_serviceTask.IsCompleted)
            {
                _cancelToken.Cancel();

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


            return Task.CompletedTask;
        }


    }
}
