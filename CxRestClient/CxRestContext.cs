using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace CxRestClient
{
    public class CxRestContext
    {
        private static String LOGIN_URI_SUFFIX = "cxrestapi/auth/identity/connect/token";
        private static String CLIENT_SECRET = "014DF517-39D1-4453-B7B3-9930C563627C";

        public class ClientFactory
        {
            private ClientFactory ()
            { }

            internal ClientFactory (String mediaType, CxRestContext ctx)
            {
                Context = ctx;
                MediaType = mediaType;
            }

            private CxRestContext Context { get; set; }
            private  String MediaType { get; set; }

            public HttpClient CreateClient()
            {
                HttpClient retVal = MakeClient(Context.ValidateSSL);

                retVal.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(Context.Token.TokenType, 
                    Context.Token.Token);

                retVal.DefaultRequestHeaders.Accept.Clear();

                retVal.DefaultRequestHeaders.Accept.Add
                    (new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json", 1.0));

                return retVal;
            }
        }

        internal CxRestContext ()
        { }

        public bool ValidateSSL { get; internal set; }
        public String Url { get; internal set; }
        public TimeSpan Timeout { get; internal set; }
        public ClientFactory Json { get; internal set; }


        private Object _tokenLock = new object();
        private LoginToken _token;
        internal LoginToken Token
        {
            get
            {
                lock(_tokenLock)
                {
                    if (DateTime.Now.CompareTo(Token.ExpireTime) >= 0)
                        _token = GetLoginToken(Url, _token.ReauthContent, ValidateSSL);
                }

                return _token;
            }

            set
            {
                _token = value;
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
        public static String MakeUrl (String url, String suffix)
        {
            if (url.EndsWith('/'))
                return url + suffix;
            else
                return url + '/' + suffix;

        }

        public static String MakeQueryString (Dictionary<String, String> query)
        {
            LinkedList<String> p = new LinkedList<string>();

            foreach (String k in query.Keys)
                p.AddLast(String.Format("{0}={1}", k, query[k]));

            return String.Join('&', p);
        }

        public static String MakeUrl(String url, String suffix, Dictionary<String, String> query)
        => MakeUrl(url, suffix) + "?" + MakeQueryString (query);


        private static LoginToken GetLoginToken(String url, HttpContent authContent, bool doSSLValidate)
        {
            HttpClient c = MakeClient(doSSLValidate);

            var uri = new Uri(MakeUrl(url, LOGIN_URI_SUFFIX));

            var response = c.PostAsync(uri, authContent).Result;

            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException(response.ReasonPhrase);

            JsonReader r = new JsonTextReader(new StreamReader
                (response.Content.ReadAsStreamAsync().Result));

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

        private static LoginToken GetLoginToken (String url, String username, String password, 
            bool doSSLValidate)
        {
            var requestContent = new Dictionary<string, string>()
            {
                {"username", username},
                {"password", password},
                {"grant_type", "password"},
                {"scope", "sast_rest_api"},
                {"client_id", "resource_owner_client"},
                {"client_secret", CLIENT_SECRET}
            };

            var payloadContent = new FormUrlEncodedContent(requestContent);

            return GetLoginToken(url, payloadContent, doSSLValidate);
        }

        private static HttpClient MakeClient (bool doSSLValidate)
        {
            HttpClientHandler h = new HttpClientHandler();
            if (!doSSLValidate)
                h.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;

            return new HttpClient(h, true);
        }

        #endregion



        public class CxRestContextBuilder
        {

            private String _url;
            public CxRestContextBuilder serviceUrl (String url)
            {
                _url = url;
                return this;
            }


            private int _timeout = 600;
            public CxRestContextBuilder withOpTimeout (int seconds)
            {
                _timeout = seconds;
                return this;
            }

            private bool _validate = true;
            public CxRestContextBuilder withSSLValidate (bool validate)
            {
                _validate = validate;
                return this;
            }


            private String _user;
            public CxRestContextBuilder withUsername (String username)
            {
                _user = username;
                return this;
            }

            private String _pass;
            public CxRestContextBuilder withPassword(String pass)
            {
                _pass = pass;
                return this;
            }


            public CxRestContext build()
            {
                if (_url == null)
                    throw new InvalidOperationException("Endpoint URL was not specified.");

                if (_user == null)
                    throw new InvalidOperationException("Username was not specified.");

                if (_pass == null)
                    throw new InvalidOperationException("Password was not specified.");

                CxRestContext retVal = new CxRestContext()
                {
                    Token = GetLoginToken(_url, _user, _pass, _validate),
                    Url = _url,
                    ValidateSSL = _validate,
                    Timeout = new TimeSpan(0, 0, _timeout)
                };

                retVal.Json = new ClientFactory("application/json", retVal);

                return retVal;
            }
        }



    }
}
