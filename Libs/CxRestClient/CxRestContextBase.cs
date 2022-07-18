using CxRestClient.Interfaces;
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
	public abstract class CxRestContextBase : ICxRestContext
	{

		private static ILog _log = LogManager.GetLogger(typeof(CxRestContextBase));

		#region Public/Private Properties

		public bool ValidateSSL { get; internal set; }
		public abstract String LoginUrl { get; }
        public abstract String ApiUrl { get; }
        public TimeSpan Timeout { get; internal set; }
		public int RetryLoop { get; internal set; }
        #endregion


        #region Context Control Methods

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void Reauthenticate()
        {
            Token.ExpireTime = DateTime.MinValue;
        }

        public abstract LoginToken Token { get; }


        [MethodImpl(MethodImplOptions.Synchronized)]
        protected virtual void ValidateToken(ref LoginToken token)
        {
                if (DateTime.Now.CompareTo(token.ExpireTime) >= 0)
                    token = GetLoginToken(token.ReauthContent);
        }

        protected LoginToken GetLoginToken(HttpContent authContent)
        {
            try
            {
                HttpClient c = HttpClientSingleton.GetClient();

                var uri = new Uri(LoginUrl);

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

        protected LoginToken GetNullToken(HttpContent authContent)
        {
            return new LoginToken
            {
                TokenType = null,
                ExpireTime = DateTime.MinValue,
                Token = null,
                ReauthContent = authContent
            };
        }


        protected LoginToken GetLoginToken(Dictionary<string, string> body)
        {
            using (var content = new FormUrlEncodedContent(body))
                return GetLoginToken(content);
        }

        #endregion

    }
}
