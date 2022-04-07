using System;
using System.Collections.Generic;
using log4net;
using CxRestClient.IO;
using System.Runtime.CompilerServices;

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
        public CxSASTClientFactory Json { get; internal set; }
        public CxSASTClientFactory Xml { get; internal set; }

        private readonly Object _tokenLock = new object();


        private LoginToken _sastToken;
        internal LoginToken SastToken
        {
            get
            {
                ValidateToken(MakeUrl(Url, LOGIN_URI_SUFFIX), ref _sastToken);
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
                ValidateToken(MakeUrl(Url, LOGIN_URI_SUFFIX), ref _mnoToken);
                return _mnoToken;
            }

            set
            {
                _mnoToken = value.GetValueOrDefault();
            }
        }


		#endregion



		[MethodImpl(MethodImplOptions.Synchronized)]
        public override void Reauthenticate()
		{
			_sastToken.ExpireTime = DateTime.MinValue;
			_mnoToken.ExpireTime = DateTime.MinValue;
		}



		#region Static utility methods



        private static LoginToken GetSASTLoginToken(String loginUrl, String username, String password)
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


            return GetLoginToken(loginUrl, requestContent);
        }


        private static LoginToken GetMNOLoginToken(String loginUrl, String username, String password)
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

            return GetLoginToken(loginUrl, requestContent);
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
                    SastToken = GetSASTLoginToken(MakeUrl(_url, LOGIN_URI_SUFFIX), _user, _pass),
                    MNOToken =  String.IsNullOrEmpty(_mnoUrl) ? new Nullable<LoginToken> () : GetMNOLoginToken(MakeUrl(_url, LOGIN_URI_SUFFIX), _user, _pass),
                    Url = _url,
                    MnoUrl = _mnoUrl,
                    ValidateSSL = _validate,
                    Timeout = timeoutSpan,
                    RetryLoop = _retryLoop
                };

                retVal.Json = new CxSASTClientFactory("application/json", retVal);
                retVal.Xml = new CxSASTClientFactory("application/xml", retVal);

                return retVal;
            }
        }
    }
}
