using log4net;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.IO
{
    internal class LoggingClientHandler : HttpClientHandler
    {
        private static ILog _log = LogManager.GetLogger(typeof(LoggingClientHandler));


        private Task<HttpResponseMessage> Intercept 
            (HttpRequestMessage request, CancellationToken cancellationToken, Guid trackGuid)
        {
            var retVal = new Task<HttpResponseMessage>(new Func<HttpResponseMessage> ( () => {

                var resp = base.SendAsync(request, cancellationToken).Result;
                _log.NetworkTraceFormat("RESPONSE {0}: Code [{1}] Reason [{2}]", trackGuid, 
                    resp.StatusCode, resp.ReasonPhrase);

                _log.NetworkTraceFormat("Headers {1}: {0}", resp.Headers.ToString(), trackGuid);

                if (resp.Content != null)
                {
                    _log.NetworkTraceFormat("Content {0}: [{1}]", trackGuid,
                        resp.Content.ReadAsStringAsync().Result);
                }

                return resp;
            }), cancellationToken);

            retVal.Start();

            return retVal;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Guid trackGuid = Guid.NewGuid();

            _log.NetworkTraceFormat("REQUEST {2}: [{0}] to [{1}]", request.Method, request.RequestUri, trackGuid);
            _log.NetworkTraceFormat("Headers {1}: {0}", request.Headers.ToString(), trackGuid);
            if (request.Content != null)
                _log.NetworkTraceFormat ("Content {0}: [{1}]", trackGuid, 
                    request.Content.ReadAsStringAsync().Result);

            return Intercept(request, cancellationToken, trackGuid);
        }
    }
}
