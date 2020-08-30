using log4net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;

[assembly: InternalsVisibleTo("CxRestClient_Tests")]
namespace CxRestClient.Utility
{
    internal class HttpClientSingleton
    {
        private static HttpClient _client = null;
        private static Object _lock = new object();

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


        public static void Initialize(bool doSSLValidate)
        {
            // TODO: Flag for trace so that the HttpClientHandler can dump request/response payloads
            // If log4net is in debug mode, intercept the traffic and dump it to the log output.
            lock (_lock)
            {
                if (_client != null)
                    throw new InvalidOperationException("HttpClient is already initialized.");

                HttpClientHandler h = new HttpClientHandler();
                if (!doSSLValidate)
                    h.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;

                _client = new HttpClient(h, true);
            }
        }

        public static HttpClient GetClient()
        {
            if (_client == null)
                throw new InvalidOperationException("HttpClient has not been initialized.");

            return _client;
        }

    }
}
