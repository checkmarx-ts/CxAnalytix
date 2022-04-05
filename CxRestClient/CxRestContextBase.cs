using CxRestClient.IO;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;

namespace CxRestClient
{
	public abstract class CxRestContextBase
	{

		private static ILog _log = LogManager.GetLogger(typeof(CxRestContextBase));

		#region Public/Private Properties

		public bool ValidateSSL { get; internal set; }
		public String Url { get; internal set; }
		public TimeSpan Timeout { get; internal set; }
		public int RetryLoop { get; internal set; }
        #endregion


        internal struct LoginToken
        {
            public String TokenType { get; internal set; }
            public DateTime ExpireTime { get; internal set; }
            public String Token { get; internal set; }
            internal HttpContent ReauthContent { get; set; }
        }



        #region Context Control Methods
        
        public abstract void Reauthenticate();


        [MethodImpl(MethodImplOptions.Synchronized)]
        internal void ValidateToken(String loginUrl, ref LoginToken token)
        {
                if (DateTime.Now.CompareTo(token.ExpireTime) >= 0)
                    token = GetLoginToken(loginUrl, token.ReauthContent);
        }

        internal static LoginToken GetLoginToken(String loginUrl, HttpContent authContent)
        {
            try
            {
                HttpClient c = HttpClientSingleton.GetClient();

                var uri = new Uri(loginUrl);

                _log.Debug($"Login URL: {uri}");

                var response = c.PostAsync(uri, authContent).Result;

                if (response.StatusCode != HttpStatusCode.OK)
                {
                    if (_log.IsDebugEnabled)
                        _log.Debug($"Response code [{response.StatusCode}]: " +
                            $"{response.Content.ReadAsStringAsync().Result}");
                    throw new InvalidOperationException(response.ReasonPhrase);
                }
                else
                    _log.Debug("Successful login.");


                using (JsonReader r = new JsonTextReader(new StreamReader
                    (response.Content.ReadAsStreamAsync().Result)))
                {

                    var results = JsonSerializer.Create().Deserialize(r,
                        typeof(Dictionary<String, String>)) as Dictionary<String, String>;

                    return new LoginToken
                    {
                        TokenType = results["token_type"],
                        ExpireTime = DateTime.Now.AddSeconds(Convert.ToDouble(results["expires_in"])),
                        Token = results["access_token"],
                        ReauthContent = authContent
                    };
                }
            }
            catch (HttpRequestException hex)
            {
                // Next operation will try to log in again due to expired token.
                _log.Warn("Unable to obtain login token due to a communication problem. " +
                    "Login will retry on next operation.", hex);
                return GetNullToken(authContent);
            }
            catch (Exception ex)
            {
                _log.Warn("Unable to obtain login token due to an unexpected exception.  " +
                    "Login will retry on the next operation.", ex);
                return GetNullToken(authContent);
            }

        }

        internal static LoginToken GetNullToken(HttpContent authContent)
        {
            return new LoginToken
            {
                TokenType = null,
                ExpireTime = DateTime.MinValue,
                Token = null,
                ReauthContent = authContent
            };
        }


        internal static LoginToken GetLoginToken(String loginUrl, Dictionary<string, string> headers)
        {
            return GetLoginToken(loginUrl, new FormUrlEncodedContent(headers));
        }



        #endregion


        #region Static utility methods
        public static String MakeUrl(String url, String suffix)
        {
            if (url.EndsWith('/'))
                return url + suffix;
            else
                return url + '/' + suffix;

        }

        public static String MakeQueryString(Dictionary<String, String> query)
        {
            LinkedList<String> p = new LinkedList<string>();

            foreach (String k in query.Keys)
                p.AddLast(String.Format("{0}={1}", k, query[k]));

            return String.Join('&', p);
        }

        public static String MakeUrl(String url, String suffix, Dictionary<String, String> query)
        => MakeUrl(url, suffix) + ((query.Count > 0) ? ("?" + MakeQueryString(query)) : (""));

        #endregion

    }
}
