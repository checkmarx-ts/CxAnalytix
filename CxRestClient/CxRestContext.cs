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
    public class CxRestContext
    {
        private static readonly String LOGIN_URI_SUFFIX = "cxrestapi/auth/identity/connect/token";
        private static readonly String CLIENT_SECRET = "014DF517-39D1-4453-B7B3-9930C563627C";

        private static readonly String SAST_SCOPE = "sast_rest_api";
        private static readonly String MNO_SCOPE = "cxarm_api";


        private static ILog _log = LogManager.GetLogger(typeof(CxRestContext));

        internal CxRestContext()
        { }

        #region Public/Private Properties

        public bool ValidateSSL { get; internal set; }
        public String Url { get; internal set; }
        public String MnoUrl { get; internal set; }
        public TimeSpan Timeout { get; internal set; }
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
        internal LoginToken MNOToken
        {
            get
            {
                ValidateToken(ref _mnoToken);
                return _mnoToken;
            }

            set
            {
                _mnoToken = value;
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

        public void Reauthenticate()
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


        #region Static methods
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
        => MakeUrl(url, suffix) + "?" + MakeQueryString(query);


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

        private static LoginToken GetLoginToken(String url, String username, String password,
            String scope)
        {
            _log.Debug($"Obtainting login token for scope [{scope}]");

            var requestContent = new Dictionary<string, string>()
            {
                {"username", username},
                {"password", password},
                {"grant_type", "password"},
                {"scope", scope},
                {"client_id", "resource_owner_client"},
                {"client_secret", CLIENT_SECRET}
            };

            var payloadContent = new FormUrlEncodedContent(requestContent);

            return GetLoginToken(url, payloadContent);
        }

        #endregion



        public class CxRestContextBuilder
        {

            private String _url;
            public CxRestContextBuilder WithSASTServiceURL(String url)
            {
                _url = url;
                return this;
            }

            private String _mnoUrl;
            public CxRestContextBuilder WithMNOServiceURL(String url)
            {
                _mnoUrl = url;
                return this;
            }


            private int _timeout = 600;
            public CxRestContextBuilder WithOpTimeout(int seconds)
            {
                _timeout = seconds;
                return this;
            }

            private bool _validate = true;
            public CxRestContextBuilder WithSSLValidate(bool validate)
            {
                _validate = validate;
                return this;
            }


            private String _user;
            public CxRestContextBuilder WithUsername(String username)
            {
                _user = username;
                return this;
            }

            private String _pass;
            public CxRestContextBuilder WithPassword(String pass)
            {
                _pass = pass;
                return this;
            }


            public CxRestContext Build()
            {
                if (_url == null)
                    throw new InvalidOperationException("Endpoint URL was not specified.");

                if (_user == null)
                    throw new InvalidOperationException("Username was not specified.");

                if (_pass == null)
                    throw new InvalidOperationException("Password was not specified.");

                var timeoutSpan = new TimeSpan(0, 0, _timeout);

                HttpClientSingleton.Initialize(_validate, timeoutSpan);

                CxRestContext retVal = new CxRestContext()
                {
                    SastToken = GetLoginToken(_url, _user, _pass, SAST_SCOPE),
                    MNOToken = GetLoginToken(_url, _user, _pass, $"{MNO_SCOPE} {SAST_SCOPE}"),
                    Url = _url,
                    MnoUrl = String.IsNullOrEmpty (_mnoUrl) ? _url : _mnoUrl,
                    ValidateSSL = _validate,
                    Timeout = timeoutSpan
                };

                retVal.Json = new CxClientFactory("application/json", retVal);
                retVal.Xml = new CxClientFactory("application/xml", retVal);

                return retVal;
            }
        }
    }
}
