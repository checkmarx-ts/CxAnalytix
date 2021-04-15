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

		private static int RETRY_DELAY_MS = 3000;
		private static int RETRY_DELAY_INCREASE_FACTOR = 2;


		private static T ExecuteOperation<T>(Func<CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess,
			Func<CxRestClient.IO.CxRestClient, HttpResponseMessage> opExecutor, CxRestContext ctx, CancellationToken token,
			Func<HttpResponseMessage, Boolean> responseErrorLogic, Func<Exception, Boolean> exceptionErrorLogic)
		{
			var endRetryAt = DateTime.Now.Add(ctx.Timeout);

			int delay = RETRY_DELAY_MS;

			DateTime? recoveryStartedAt = null;
			bool inRecovery = false;
			HttpStatusCode lastFailCode = HttpStatusCode.OK;

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
						else if (response.StatusCode == HttpStatusCode.NotFound)
						{
							_log.Warn($"{response.StatusCode} error with URI {response.RequestMessage.RequestUri}, not attempting retry.");

							return default(T);
						}
						else
						{
							if (!recoveryStartedAt.HasValue)
								recoveryStartedAt = DateTime.Now;

							if (responseErrorLogic != null && !responseErrorLogic(response))
								return default(T);

							if (!inRecovery)
								_log.Warn($"Request failed with response {Convert.ToInt32(response.StatusCode)}({response.StatusCode})" + 
									$" - Retrying until {endRetryAt}");
							else if (lastFailCode != response.StatusCode)
								_log.Warn($"Still in recovery, new failure status code: " 
									+ $"{Convert.ToInt32(response.StatusCode)}({response.StatusCode})" +
									$" - Retrying until {endRetryAt}");

							inRecovery = true;
							lastFailCode = response.StatusCode;

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

					if (!inRecovery)
						_log.Error($"Exception: {ex.GetType()} Source: {ex.Source} Message: {ex.Message}" +
							$" - Retrying until {endRetryAt}");

					inRecovery = true;

					nonRecoveryException = new UnrecoverableOperationException($"Last exception caught", ex);

					if (exceptionErrorLogic != null && !exceptionErrorLogic(ex))
						throw ex;
				}

				_log.Debug($"Waiting {delay}ms before retry.");
				Task.Delay(delay, token).Wait();
				delay *= RETRY_DELAY_INCREASE_FACTOR;
			}

			throw nonRecoveryException;
		}

		public static T ExecuteGet<T>(Func<CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess,
			String url, CxRestContext ctx, CancellationToken token, Func<HttpResponseMessage, Boolean> responseErrorLogic = null,
			Func<Exception, Boolean> exceptionErrorLogic = null)
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
				, responseErrorLogic
				, exceptionErrorLogic);
		}


		public static T ExecutePost<T>(Func<CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess,
			String url, Func<HttpContent> contentFactory, CxRestContext ctx, CancellationToken token,
			Func<HttpResponseMessage, Boolean> responseErrorLogic = null, Func<Exception, Boolean> exceptionErrorLogic = null)
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
				, responseErrorLogic
				, exceptionErrorLogic);
		}

	}
}
