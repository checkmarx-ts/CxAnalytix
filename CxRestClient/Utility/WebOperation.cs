using CxAnalytix.Exceptions;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.Utility
{
	internal class WebOperation
	{
		private static ILog _log = LogManager.GetLogger(typeof(WebOperation));

		private static int RETRY_DELAY_MS = 1000;
		private static int RETRY_DELAY_INCREASE_FACTOR = 2;


		private static T ExecuteOperation<T>(Func<CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess,
			Func<CxRestClient.IO.CxRestClient, HttpResponseMessage> opExecutor, CxRestContext ctx, CancellationToken token,
			Func<HttpResponseMessage, Boolean> errorLogic)
		{
			var endRetryAt = DateTime.Now.Add(ctx.Timeout);

			int delay = RETRY_DELAY_MS;

			DateTime? recoveryStartedAt = null;
			bool inRecovery = false;

			UnrecoverableOperationException nonRecoveryException = new UnrecoverableOperationException();


			while (DateTime.Now.CompareTo(endRetryAt) <= 0)
			{
				try
				{
					using (var client = clientFactory())
					using (var response = opExecutor(client))
					{
						if (token.IsCancellationRequested)
						{
							_log.Warn($"Execution of operation has been cancelled.");
							return default(T);
						}


						if (response.IsSuccessStatusCode)
						{
							if (inRecovery)
								_log.Info($"Operation successful after last error - "
									+ $"recovered after {DateTime.Now.Subtract(recoveryStartedAt.Value).TotalMilliseconds}ms ");

							return onSuccess(response);
						}
						else
						{
							if (!recoveryStartedAt.HasValue)
								recoveryStartedAt = DateTime.Now;

							if (errorLogic != null && !errorLogic(response))
								return default(T);

							if (!inRecovery)
								_log.Warn($"Request failed with response {Convert.ToInt32(response.StatusCode)}({response.StatusCode})" + 
									$" - Retrying until {endRetryAt}");

							inRecovery = true;

							nonRecoveryException = new UnrecoverableOperationException(response.StatusCode, 
								response.RequestMessage.RequestUri);

							switch (response.StatusCode)
							{
								case HttpStatusCode.Unauthorized:
								case HttpStatusCode.Forbidden:
									ctx.Reauthenticate();
									break;
								default:
									break;
							}
						}
					}
				}
				catch (Exception ex)
				{
					if (!recoveryStartedAt.HasValue)
						recoveryStartedAt = DateTime.Now;

					inRecovery = true;

					nonRecoveryException = new UnrecoverableOperationException($"Last exception caught", ex);

					_log.Error($"Exception: {ex.GetType()} Source: {ex.Source} Message: {ex.Message}" +
						$" - Retrying until {endRetryAt}");
				}

				Task.Delay(delay, token).Wait();
				delay *= RETRY_DELAY_INCREASE_FACTOR;
			}

			throw nonRecoveryException;
		}

		public static T ExecuteGet<T>(Func<CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess,
			String url, CxRestContext ctx, CancellationToken token, Func<HttpResponseMessage, Boolean> errorLogic = null)
		{
			return ExecuteOperation<T>(
				clientFactory
				, onSuccess
				, (client) =>
				{
					_log.Debug($"Executing GET operation at {url}");
					return client.GetAsync(url, token).Result;
				}
				, ctx
				, token
				, errorLogic);
		}


		public static T ExecutePost<T>(Func<CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess,
			String url, Func<HttpContent> contentFactory, CxRestContext ctx, CancellationToken token,
			Func<HttpResponseMessage, Boolean> errorLogic = null)
		{
			return ExecuteOperation<T>(
				clientFactory
				, onSuccess
				, (client) =>
				{
					_log.Debug($"Executing POST operation at {url}");

					// HttpClient.SendAsync disposes of the payload on send
					// this means if there is an error, a new instance is needed
					// on retry.
					return client.PostAsync(url, (contentFactory != null) ? contentFactory() : null, token).Result;
				}
				, ctx
				, token
				, errorLogic);
		}

	}
}
