using log4net;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("CxRestClient_Tests")]
namespace CxRestClient.IO
{
    internal class HttpClientSingleton
    {
        private static HttpClient _client = null;
        private static readonly Object _lock = new object();
        private static ProductInfoHeaderValue _userAgent;

        private static ILog _log = LogManager.GetLogger(typeof(HttpClientSingleton));

		static HttpClientSingleton()
		{
            var agent = CxAnalytix.Utilities.Reflection.GetUserAgentName();

            try
            {
                _userAgent = new ProductInfoHeaderValue($"{agent.CompanyName}-{agent.ProductName}", agent.ProductVersion);
                _log.Debug($"User Agent: {_userAgent}");
            }
            catch (Exception)
			{
                agent = new CxAnalytix.Utilities.Reflection.UserAgentComponents();

                _userAgent = new ProductInfoHeaderValue($"{agent.CompanyName}-{agent.ProductName}", agent.ProductVersion);
                // Attempting to assign values such as "Microsoft Corporation" causes the
                // user agent class to throw an exception.
			}
        }


        private HttpClientSingleton()
        { }

        public static void Clear()
        {
            lock (_lock)
                if (_client != null)
                {
                    _client.Dispose();
                    _client = null;
                }
        }


        public static void Initialize(bool doSSLValidate, TimeSpan opTimeout)
        {
            lock (_lock)
            {
                if (_client != null)
                    return;

                HttpClientHandler h = GetClientHandler();
                if (!doSSLValidate)
                    h.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;


                _client = new HttpClient(h, true);
                _client.Timeout = opTimeout;
                _client.DefaultRequestHeaders.UserAgent.Add(_userAgent);
            }
        }

        public static void Initialize(bool doSSLValidate, TimeSpan opTimeout, String additionalUserAgent)
        {
            Initialize(doSSLValidate, opTimeout);

            lock (_lock)
            {
                _client.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue(additionalUserAgent, null));
            }
        }

        private static HttpClientHandler GetClientHandler ()
        {
            if (!_log.IsNetworkTrace())
                return new HttpClientHandler();
            else
                return new LoggingClientHandler();



        }

        public static HttpClient GetClient()
        {
            lock(_lock)
				if (_client == null)
					throw new InvalidOperationException("HttpClient has not been initialized.");

			return _client;
        }

    }
}
