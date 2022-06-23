using CxAnalytix.Configuration;
using CxAnalytix.TransformLogic;
using CxRestClient;
using log4net;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
using CxAnalytix.AuditTrails.Crawler;
using CxAnalytix.Exceptions;
using ProjectFilter;
using CxRestClient.Utility;
using CxAnalytix.Executive;

namespace CxAnalytixDaemon
{
	class Daemon : IHostedService, IDisposable
	{
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
			_cancelToken = new CancellationTokenSource();

			_serviceTask = Task.Run( () =>
			{
                ExecuteLoop.Execute(_cancelToken);

			}, _cancelToken.Token);

			return Task.CompletedTask;
		}

		public Task StopAsync(CancellationToken cancellationToken)
		{
			if (_cancelToken != null && _serviceTask != null && !_serviceTask.IsCompleted)
			{
				_cancelToken.Cancel();

				try
				{
					_serviceTask.Wait(cancellationToken);
				}
				catch (AggregateException)
				{
				}
			}

			return Task.CompletedTask;
		}


	}
}
