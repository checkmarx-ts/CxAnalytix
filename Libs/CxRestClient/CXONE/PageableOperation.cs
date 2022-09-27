using CxRestClient.Interfaces;
using CxRestClient.Utility;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.CXONE
{
    internal static class PageableOperation
    {
        private static readonly int DEFAULT_PAGESIZE = 100;

        private class PagingContext
        {
            private int _retry = 0;
            private int _maxRetry;
            private TimeSpan _timeoutLength;
            private DateTime _currentEnd;
            private CancellationToken _token;
            private int _offset = 0;
            private int _pageSize;

            private string _offsetKey;
            private string _limitKey;

            private Dictionary<String, String> _headers;

            public PagingContext(ICxRestContext ctx, CancellationToken token, String offsetKey, String limitKey, int pageSize)
            {
                _maxRetry = ctx.RetryLoop;
                _timeoutLength = ctx.Timeout;
                _currentEnd = DateTime.Now.Add(_timeoutLength);
                _token = token;
                _offsetKey = offsetKey;
                _limitKey = limitKey;
                _headers = new() { { _offsetKey, "0" }, { _limitKey, Convert.ToString(pageSize) } };
                _pageSize = pageSize;
            }

            private void IncrementRetry()
            {
                _retry++;
                if (_retry < _maxRetry)
                    _currentEnd = DateTime.Now.Add(_timeoutLength);
            }

            private bool IsTimedOut() => DateTime.Now.CompareTo(_currentEnd) > 0;

            public bool ShouldAbort
            {
                get
                {
                    if (IsTimedOut())
                        IncrementRetry();

                    return _token.IsCancellationRequested || IsTimedOut() || LastReturnCount < _pageSize;
                }
            }

            public int LastReturnCount { private get; set; }

            public Dictionary<String, String> NextPage()
            {
                _headers[_offsetKey] = Convert.ToString(_offset);
                _offset += _pageSize;
                return _headers;
            }
        }



        internal static Task DoPagedGetRequest<T>(Func<T, int> accmulator, Func<String, CxRestClient.IO.CxRestClient> clientFactory,
            Func<HttpResponseMessage, T> onSuccess, String url, ICxRestContext ctx, CancellationToken token,
            Func<HttpResponseMessage, Boolean> responseErrorLogic = null, Func<Exception, Boolean> exceptionErrorLogic = null,
            String apiVersion = "1.0", Func<T, bool> finishedEval = null, String offsetKey = "offset", String limitKey = "limit", 
            int? pageSize = null)
        {

            if (!pageSize.HasValue)
                pageSize = DEFAULT_PAGESIZE;

            return Task.Run(() =>
            {

                var pager = new PagingContext(ctx, token, offsetKey, limitKey, pageSize.Value);

                do
                {
                    var newUrl = UrlUtils.MakeUrl(url, pager.NextPage());

                    var response = WebOperation.ExecuteGet<T>(clientFactory, onSuccess, newUrl, ctx, token, responseErrorLogic, exceptionErrorLogic, apiVersion);

                    pager.LastReturnCount = accmulator(response);

                    if (finishedEval != null && finishedEval(response))
                        break;

                } while (!pager.ShouldAbort);

            });

        }
    }
}
