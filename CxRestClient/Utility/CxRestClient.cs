using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.Utility
{
    public class CxRestClient : IDisposable
    {
        private HttpRequestMessage _msg;

        internal CxRestClient(AuthenticationHeaderValue authHeader, String acceptMediaType)
        {
            _msg = new HttpRequestMessage();
            _msg.Headers.Authorization = authHeader;
            _msg.Headers.Add("Accept", acceptMediaType);
        }

        private Task<HttpResponseMessage> DoAsyncOp(HttpMethod method, CancellationToken cancelToken)
        {
            _msg.Method = method;
            return HttpClientSingleton.GetClient().SendAsync(_msg, cancelToken);
        }

        public Task<HttpResponseMessage> PostAsync(Uri dest, HttpContent content, CancellationToken cancelToken)
        {
            _msg.Content = content;
            _msg.RequestUri = dest;
            return DoAsyncOp(HttpMethod.Post, cancelToken);
        }

        public Task<HttpResponseMessage> PostAsync(String dest, HttpContent content, CancellationToken cancelToken) =>
            PostAsync(new Uri(dest), content, cancelToken);

        public Task<HttpResponseMessage> GetAsync(Uri dest, CancellationToken cancelToken)
        {
            _msg.RequestUri = dest;
            return DoAsyncOp(HttpMethod.Get, cancelToken);
        }

        public Task<HttpResponseMessage> GetAsync(String dest, CancellationToken cancelToken) =>
            GetAsync(new Uri (dest), cancelToken);

        public void Dispose()
        {
            _msg.Dispose();
            _msg = null;
        }
    }
}
