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



        public class CxSASTRestContextBuilder : CxRestContextBuilderCommon<CxSASTRestContextBuilder>
        {


            private String _mnoUrl;
            public CxSASTRestContextBuilder WithMNOServiceURL(String url)
            {
                _mnoUrl = url;
                return this;
            }

            private bool _validate = true;
            public CxSASTRestContextBuilder WithSSLValidate(bool validate)
            {
                _validate = validate;
                return this;
            }

			internal override void Validate()
			{
				base.Validate();

                if (!String.IsNullOrEmpty(_mnoUrl) && !Uri.IsWellFormedUriString(_mnoUrl, UriKind.Absolute) )
                    throw new InvalidOperationException("M&O Endpoint URL is invalid.");

            }

            public CxSASTRestContext Build()
            {
                Validate();

                var timeoutSpan = new TimeSpan(0, 0, Timeout);

                HttpClientSingleton.Initialize(_validate, timeoutSpan);

                CxSASTRestContext retVal = new CxSASTRestContext()
                {
                    SastToken = GetSASTLoginToken(MakeUrl(Url, LOGIN_URI_SUFFIX), User, Password),
                    MNOToken =  String.IsNullOrEmpty(_mnoUrl) ? new Nullable<LoginToken> () : GetMNOLoginToken(MakeUrl(Url, LOGIN_URI_SUFFIX), User, Password),
                    Url = Url,
                    MnoUrl = _mnoUrl,
                    ValidateSSL = _validate,
                    Timeout = timeoutSpan,
                    RetryLoop = RetryLoop
                };

                retVal.Json = new CxSASTClientFactory("application/json", retVal);
                retVal.Xml = new CxSASTClientFactory("application/xml", retVal);

                return retVal;
            }
        }
    }
}
