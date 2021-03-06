﻿using CxAnalytix.Configuration;
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
using CxAnalytix.Exceptions;
using ProjectFilter;

namespace CxAnalytixDaemon
{
	class Daemon : IHostedService, IDisposable
	{
		private static ILog _log = LogManager.GetLogger(typeof(Daemon));
		private CancellationTokenSource _cancelToken;
		private Task _serviceTask;


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

			var builder = new CxRestContext.CxRestContextBuilder();
			builder.WithSASTServiceURL(Config.Connection.URL).
			WithMNOServiceURL(Config.Connection.MNOUrl)
			.WithOpTimeout(Config.Connection.TimeoutSeconds)
			.WithSSLValidate(Config.Connection.ValidateCertificates)
			.WithUsername(Config.Credentials.Username)
			.WithPassword(Config.Credentials.Password);

			var restCtx = builder.Build();

			_cancelToken = new CancellationTokenSource();

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
						}, _cancelToken.Token);
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
						if (!cancellationToken.IsCancellationRequested)
							AuditTrailCrawler.CrawlAuditTrails(_cancelToken.Token);
					}
					catch (ProcessFatalException pfe)
					{
						_log.Error("Fatal exception caught, program ending.", pfe);
						Program._tokenSrc.Cancel();
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

			return Task.CompletedTask;
		}

		private static void Fatal(Exception ex)
		{
			_log.Error("Fatal exception caught, program ending.", ex);
			Program._tokenSrc.Cancel();
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			if (_cancelToken != null && _serviceTask != null && !_serviceTask.IsCompleted)
			{
				_log.Debug("Waiting for the service task to complete after cancellation.");

				_cancelToken.Cancel();

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


			return Task.CompletedTask;
		}


	}
}
