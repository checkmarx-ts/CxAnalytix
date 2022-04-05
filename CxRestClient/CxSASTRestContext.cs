using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using log4net;
using CxRestClient.IO;

namespace CxRestClient
{
    public class CxSASTRestContext : CxRestContextBase
    {
        private static readonly String LOGIN_URI_SUFFIX = "cxrestapi/auth/identity/connect/token";
        private static readonly String CLIENT_SECRET = "014DF517-39D1-4453-B7B3-9930C563627C";

        private static ILog _log = LogManager.GetLogger(typeof(CxSASTRestContext));

        internal CxSASTRestContext()
        { }

        #region Public/Private Properties

        public String MnoUrl { get; internal set; }
        public CxClientFactory Json { get; internal set; }
        public CxClientFactory Xml { get; internal set; }


        private readonly Object _tokenLock = new object();


        private LoginToken _sastToken;
        internal LoginToken SastToken
        {
            get
            {
                ValidateToken(ref _sastToken);
                return _sastToken;
            }

            set
            {
                _sastToken = value;
            }
        }

        private LoginToken _mnoToken;
        internal LoginToken? MNOToken
        {
            get
            {
                ValidateToken(ref _mnoToken);
                return _mnoToken;
            }

            set
            {
                _mnoToken = value.GetValueOrDefault();
            }
        }

        #endregion


        private void ValidateToken(ref LoginToken token)
        {
            lock (_tokenLock)
            {
				if (DateTime.Now.CompareTo(token.ExpireTime) >= 0)
					token = GetLoginToken(Url, token.ReauthContent);
			}
        }

        public override void Reauthenticate()
		{
            lock (_tokenLock)
            {
                _sastToken.ExpireTime = DateTime.MinValue;
                _mnoToken.ExpireTime = DateTime.MinValue;
            }
        }

        internal struct LoginToken
        {
            public String TokenType { get; internal set; }
            public DateTime ExpireTime { get; internal set; }
            public String Token { get; internal set; }
            internal HttpContent ReauthContent { get; set; }
        }


        #region Static utility methods

        private static LoginToken GetLoginToken(String url, HttpContent authContent)
        {
            try
            {
                HttpClient c = HttpClientSingleton.GetClient();

                var uri = new Uri(MakeUrl(url, LOGIN_URI_SUFFIX));

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

        private static LoginToken GetNullToken(HttpContent authContent)
        {
            return new LoginToken
            {
                TokenType = null,
                ExpireTime = DateTime.MinValue,
                Token = null,
                ReauthContent = authContent
            };
        }


        private static LoginToken GetLoginToken(String url, Dictionary<string, string> headers)
		{
            return GetLoginToken(url, new FormUrlEncodedContent(headers));
		}


        private static LoginToken GetSASTLoginToken(String url, String username, String password)
        {
            _log.Debug($"Obtainting SAST login token");

            var requestContent = new Dictionary<string, string>()
            {
                {"username", username},
                {"password", password},
                {"grant_type", "password"},
                {"scope", "sast_api"},
                {"client_id", "resource_owner_sast_client"},
                {"client_secret", CLIENT_SECRET}
            };


            return GetLoginToken(url, requestContent);
        }


        private static LoginToken GetMNOLoginToken(String url, String username, String password)
        {
            _log.Debug($"Obtainting CxARM login token");

            var requestContent = new Dictionary<string, string>()
            {
                {"username", username},
                {"password", password},
                {"grant_type", "password"},
                {"scope", "sast_rest_api cxarm_api"},
                {"client_id", "resource_owner_client"},
                {"client_secret", CLIENT_SECRET}
            };

            return GetLoginToken(url, requestContent);
        }


        #endregion



        public class CxSASTRestContextBuilder
        {

            private String _url;
            public CxSASTRestContextBuilder WithSASTServiceURL(String url)
            {
                _url = url;
                return this;
            }

            private String _mnoUrl;
            public CxSASTRestContextBuilder WithMNOServiceURL(String url)
            {
                _mnoUrl = url;
                return this;
            }


            private int _timeout = 600;
            public CxSASTRestContextBuilder WithOpTimeout(int seconds)
            {
                _timeout = seconds;
                return this;
            }

            private bool _validate = true;
            public CxSASTRestContextBuilder WithSSLValidate(bool validate)
            {
                _validate = validate;
                return this;
            }


            private String _user;
            public CxSASTRestContextBuilder WithUsername(String username)
            {
                _user = username;
                return this;
            }

            private String _pass;
            public CxSASTRestContextBuilder WithPassword(String pass)
            {
                _pass = pass;
                return this;
            }

            private int _retryLoop;
            public CxSASTRestContextBuilder WithRetryLoop(int loopCount)
			{
                _retryLoop = loopCount;
                return this;
			}


            public CxSASTRestContext Build()
            {
                if (_url == null)
                    throw new InvalidOperationException("Endpoint URL was not specified.");

                if (_user == null)
                    throw new InvalidOperationException("Username was not specified.");

                if (_pass == null)
                    throw new InvalidOperationException("Password was not specified.");

                if (_retryLoop < 0)
                    throw new InvalidOperationException("Retry loop can't be < 0.");

                if (_timeout < 0)
                    throw new InvalidOperationException("Timeout can't be < 0.");

                var timeoutSpan = new TimeSpan(0, 0, _timeout);

                HttpClientSingleton.Initialize(_validate, timeoutSpan);

                CxSASTRestContext retVal = new CxSASTRestContext()
                {
                    SastToken = GetSASTLoginToken(_url, _user, _pass),
                    MNOToken =  String.IsNullOrEmpty(_mnoUrl) ? new Nullable<LoginToken> () : GetMNOLoginToken(_url, _user, _pass),
                    Url = _url,
                    MnoUrl = _mnoUrl,
                    ValidateSSL = _validate,
                    Timeout = timeoutSpan,
                    RetryLoop = _retryLoop
                };

                retVal.Json = new CxClientFactory("application/json", retVal);
                retVal.Xml = new CxClientFactory("application/xml", retVal);

                return retVal;
            }
        }
    }
}
