using CxAnalytix.Exceptions;
using CxAnalytix.Extensions;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.Utility
{
	internal class WebOperation
	{
		private static ILog _log = LogManager.GetLogger(typeof(WebOperation));

		private static int RETRY_DELAY_MS = 3000;
		private static int RETRY_DELAY_INCREASE_FACTOR = 2;


		private static T ExecuteOperation<T>(Func<String, CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess,
			Func<CxRestClient.IO.CxRestClient, HttpResponseMessage> opExecutor, CxSASTRestContext ctx, CancellationToken token,
			Func<HttpResponseMessage, Boolean> responseErrorLogic, Func<Exception, Boolean> exceptionErrorLogic, String apiVersion)
		{
			int loopsLeft = ctx.RetryLoop;

			UnrecoverableOperationException nonRecoveryException = new UnrecoverableOperationException();

			while (loopsLeft >= 0)
			{
				if (loopsLeft != ctx.RetryLoop)
					_log.Warn($"Retry loop {ctx.RetryLoop - loopsLeft} of {ctx.RetryLoop}");

				loopsLeft--;

				nonRecoveryException = new UnrecoverableOperationException("Loop retries exhausted");

				var endRetryAt = DateTime.Now.Add(ctx.Timeout);

				int delay = RETRY_DELAY_MS;

				DateTime? recoveryStartedAt = null;
				bool inRecovery = false;
				HttpStatusCode lastFailCode = HttpStatusCode.OK;



				while (DateTime.Now.CompareTo(endRetryAt) <= 0)
				{
					try
					{
						using (var client = clientFactory(apiVersion))
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

						nonRecoveryException = new UnrecoverableOperationException($"Last exception caught: {ex.GetType().Name}: {ex.Message}");

						if (exceptionErrorLogic != null && !exceptionErrorLogic(ex))
							throw ex;
					}

					_log.Debug($"Waiting {delay}ms before retry.");
					Task.Delay(delay, token).Wait();
					delay *= RETRY_DELAY_INCREASE_FACTOR;
				}

				if (inRecovery)
					_log.Debug("Retry time exceeded, while loop exited during recovery.");
			}

			throw nonRecoveryException;
		}

		private static void LogAggregateException(AggregateException aex)
		{
			StringBuilder sb = new StringBuilder();

			aex.Handle((x) => {

				sb.AppendLine("----- EXCEPTION -----");
				if (x is UnrecoverableOperationException)
					sb.AppendLine(x.Message);
				else
					sb.AppendLine($"Type: {x.GetType().FullName} Message: {x.Message}");

				return true;
			});

			_log.Error($"Aggregate exception: {sb.ToString()}");
		}

		public static T ExecuteGet<T>(Func<String, CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess,
			String url, CxSASTRestContext ctx, CancellationToken token, Func<HttpResponseMessage, Boolean> responseErrorLogic = null,
			Func<Exception, Boolean> exceptionErrorLogic = null, String apiVersion = "1.0")
		{
			return ExecuteOperation<T>(
				clientFactory
				, onSuccess
				, (client) =>
				{
					_log.Trace($"Executing GET operation at {url}");

					try
					{
						using (new OpTimer($"GET {url}"))
						{
							var result = client.GetAsync(url, token).Result;

							_log.Trace($"GET operation at {url} status: {(int)result.StatusCode}:{result.ReasonPhrase}");

							return result;
						}
					}
					catch(AggregateException aex)
					{
						LogAggregateException(aex);
						throw aex;
					}
					catch (Exception ex)
					{
						_log.Error($"GET operation failed for [{url}]", ex);
						throw ex;
					}
				}
				, ctx
				, token
				, responseErrorLogic
				, exceptionErrorLogic
				, apiVersion);

		}


		public static T ExecutePost<T>(Func<String, CxRestClient.IO.CxRestClient> clientFactory, Func<HttpResponseMessage, T> onSuccess,
			String url, Func<HttpContent> contentFactory, CxSASTRestContext ctx, CancellationToken token,
			Func<HttpResponseMessage, Boolean> responseErrorLogic = null, Func<Exception, Boolean> exceptionErrorLogic = null, String apiVersion = "1.0")
		{
			return ExecuteOperation<T>(
				clientFactory
				, onSuccess
				, (client) =>
				{
					_log.Trace($"Executing POST operation at {url}");

					try
					{
						// HttpClient.SendAsync disposes of the payload on send
						// this means if there is an error, a new instance is needed
						// on retry.
						using (new OpTimer($"POST {url}"))
						{
							var result = client.PostAsync(url, (contentFactory != null) ? contentFactory() : null, token).Result;

							_log.Trace($"POST operation at {url} status: {(int)result.StatusCode}:{result.ReasonPhrase}");

							return result;
						}
					}
					catch (AggregateException aex)
					{
						LogAggregateException(aex);
						throw aex;
					}
					catch (Exception ex)
					{
						_log.Error($"POST operation failed for [{url}]", ex);
						throw ex;

					}
				}
				, ctx
				, token
				, responseErrorLogic
				, exceptionErrorLogic
				, apiVersion);
		}

	}
}
