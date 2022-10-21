using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;
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
            _cancelToken.Token.Register(() =>
			{
				var token = new CancellationTokenSource();

				using (var task = StopAsync(token.Token) )
				{
					if (!_serviceTask.Wait(120000))
						token.Cancel();
				}
			});


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
                if (!_cancelToken.IsCancellationRequested)
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
