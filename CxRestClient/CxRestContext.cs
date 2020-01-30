using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace CxRestClient
{
    public class CxRestContext : IDisposable
    {
        private static String URI_SUFFIX = "cxrestapi/auth/identity/connect/token";
        private static String CLIENT_SECRET = "014DF517-39D1-4453-B7B3-9930C563627C";


        internal CxRestContext ()
        { }

        internal LoginToken Token { get; set; }

        private HttpClient _client;
        private HttpClient Client
        {
            get
            {
                if (DateTime.Now.CompareTo(Token.ExpireTime) >= 0)
                    Token = GetLoginToken(Url, Token.ReauthContent);

                _client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(Token.TokenType, Token.Token);
                    
                return _client;
            }

            set
            {
                _client = value;
            }
        }

        public String Url { get; internal set; }


        internal struct LoginToken
        {
            public String TokenType { get; internal set; }
            public DateTime ExpireTime { get; internal set; }
            public String Token { get; internal set; }
            internal HttpContent ReauthContent { get; set; }
        }

        public HttpClient GetJsonClient ()
        {
            var client = Client;

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add
                (new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json", 1.0));

            return client;
        }

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


        private static LoginToken GetLoginToken(String url, HttpContent authContent)
        {
            HttpClient c = new HttpClient();

            var uri = new Uri(MakeUrl(url, URI_SUFFIX));

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

        private static LoginToken GetLoginToken (String url, String username, String password)
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

            return GetLoginToken(url, payloadContent);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

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


            public CxRestContext build ()
            {
                if (_url == null)
                    throw new InvalidOperationException("Endpoint URL was not specified.");

                if (_user == null)
                    throw new InvalidOperationException("Username was not specified.");

                if (_pass == null)
                    throw new InvalidOperationException("Password was not specified.");

                CxRestContext retVal = new CxRestContext();
                retVal.Token = GetLoginToken(_url, _user, _pass);
                retVal.Url = _url;

                HttpClientHandler h = new HttpClientHandler();
                if (!_validate)
                    h.ServerCertificateCustomValidationCallback = (msg, cert, chain, errors) => true;

                HttpClient c = new HttpClient(h, true);
                c.Timeout = new TimeSpan(0, 0, _timeout);

                retVal.Client = c;

                return retVal;
            }
        }



    }
}
