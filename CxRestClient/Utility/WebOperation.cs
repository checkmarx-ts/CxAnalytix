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
			Func<CxRestClient.IO.CxRestClient, HttpResponseMessage> opExecutor, CxRestContext ctx, CancellationToken token)
		{
			var endRetryAt = DateTime.Now.Add(ctx.Timeout);

			int delay = RETRY_DELAY_MS;

			HttpStatusCode lastFail = HttpStatusCode.OK;

			String errMsg = "";


			while (DateTime.Now.CompareTo(endRetryAt) <= 0)
			{
				try
				{
					using (var client = clientFactory())
					using (var response = opExecutor(client) )
					{
						if (token.IsCancellationRequested)
						{
							_log.Warn($"Execution of operation has been cancelled.");
							return default(T);
						}

						if (response.IsSuccessStatusCode)
							return onSuccess(response);
						else
						{
							errMsg = $"Request failed with response {Convert.ToInt32(response.StatusCode)}({response.StatusCode})";

							if (lastFail != response.StatusCode)
							{
								lastFail = response.StatusCode;
								_log.Warn($"{errMsg} - Retrying until {endRetryAt}");
							}

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
				catch (HttpRequestException hex)
				{
					_log.Error("Communication error.", hex);
					throw hex;
				}

				Task.Delay(delay, token).Wait();
				delay *= RETRY_DELAY_INCREASE_FACTOR;
			}

			throw new InvalidOperationException(errMsg);
		}

		public static T ExecuteGet<T>(Func<CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess, 
			String url, CxRestContext ctx, CancellationToken token)
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
				, token);
		}


		public static T ExecutePost<T>(Func<CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess,
			String url, HttpContent payload, CxRestContext ctx, CancellationToken token)
		{
			return ExecuteOperation<T>(
				clientFactory
				, onSuccess
				, (client) =>
				{
					_log.Debug($"Executing POST operation at {url}");
					return client.PostAsync(url, payload, token).Result; 
				}
				, ctx
				, token);
		}

	}
}
