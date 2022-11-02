using System;
using System.Collections.Generic;
using log4net;
using CxRestClient.IO;
using CxRestClient.Utility;
using System.Runtime.CompilerServices;

namespace CxRestClient
{
    public class CxSASTRestContext
    {
        private static readonly String LOGIN_URI_SUFFIX = "cxrestapi/auth/identity/connect/token";
        private static readonly String CLIENT_SECRET = "014DF517-39D1-4453-B7B3-9930C563627C";

        private static ILog _log = LogManager.GetLogger(typeof(CxSASTRestContext));

        internal CxSASTRestContext()
        { }

        #region Public/Private Properties
        public CxSimpleRestContext Sast { get; private set; }
        public CxSimpleRestContext Mno { get; private set; }

        #endregion


        #region SAST REST Context Implementation
        private class SASTRestContext : CxSimpleRestContext
        {
            public override string LoginUrl => UrlUtils.MakeUrl(_InternalUrl, LOGIN_URI_SUFFIX);

            public override string ApiUrl => _InternalUrl;

            internal String _InternalUrl { get; set; }

            private LoginToken _token = null;

            protected override LoginToken TokenImpl
            {
                get
                {
                    if (_token == null)
                        _token = GetSASTLoginToken();

                    ValidateToken(ref _token);
                    return _token;

                }
            }

            private LoginToken GetSASTLoginToken()
            {
                _log.Debug($"Obtainting SAST login token");

                var requestContent = new Dictionary<string, string>()
            {
                {"username", User},
                {"password", Password},
                {"grant_type", "password"},
                {"scope", "sast_api"},
                {"client_id", "resource_owner_sast_client"},
                {"client_secret", CLIENT_SECRET}
            };


                return GetLoginToken(requestContent);
            }
        }
        #endregion

        #region MNO REST Context Implementation
        private class MNORestContext : CxSimpleRestContext
        {
            public override string LoginUrl => UrlUtils.MakeUrl(_LoginUrl, LOGIN_URI_SUFFIX);

            public override string ApiUrl => _MnoUrl;


            internal String _MnoUrl { get; set; }
            internal String _LoginUrl { get; set; }


            private LoginToken _token = null;
            protected override LoginToken TokenImpl
            {
                get
                {
                    if (_MnoUrl == null)
                        return null;

                    if (_token == null)
                        _token = GetMNOLoginToken();

                    ValidateToken(ref _token);
                    return _token;
                }
            }

            private LoginToken GetMNOLoginToken()
            {
                _log.Debug($"Obtainting CxARM login token");

                var requestContent = new Dictionary<string, string>()
            {
                {"username", User},
                {"password", Password},
                {"grant_type", "password"},
                {"scope", "sast_rest_api cxarm_api"},
                {"client_id", "resource_owner_client"},
                {"client_secret", CLIENT_SECRET}
            };

                return GetLoginToken(requestContent);
            }

        }
        #endregion


        public class CxSASTRestContextBuilder : CxRestContextWithCredsBuilderBase<CxSASTRestContextBuilder>
        {


            private String _mnoUrl;
            public CxSASTRestContextBuilder WithMNOServiceURL(String url)
            {
                _mnoUrl = url;
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

                    Sast = new SASTRestContext()
                    {
                        User = User,
                        Password = Password,
                        _InternalUrl = ApiUrl,
                        ValidateSSL = _validate,
                        Timeout = timeoutSpan,
                        RetryLoop = RetryLoop

                    },
                    Mno = new MNORestContext()
                    {
                        User = User,
                        Password = Password,
                        _MnoUrl = _mnoUrl,
                        _LoginUrl = ApiUrl,
                        ValidateSSL = _validate,
                        Timeout = timeoutSpan,
                        RetryLoop = RetryLoop

                    }
                };

                return retVal;
            }
        }
    }
}
