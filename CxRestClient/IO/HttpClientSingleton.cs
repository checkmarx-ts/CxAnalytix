using log4net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("CxRestClient_Tests")]
namespace CxRestClient.IO
{
    internal class HttpClientSingleton
    {
        private static HttpClient _client = null;
        private static readonly Object _lock = new object();

        private static ILog _log = LogManager.GetLogger(typeof(HttpClientSingleton));

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
                    throw new InvalidOperationException("HttpClient is already initialized.");

                HttpClientHandler h = GetClientHandler();
                if (!doSSLValidate)
                    h.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;


                _client = new HttpClient(h, true);
                _client.Timeout = opTimeout;
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
            if (_client == null)
                throw new InvalidOperationException("HttpClient has not been initialized.");

            return _client;
        }

    }
}
