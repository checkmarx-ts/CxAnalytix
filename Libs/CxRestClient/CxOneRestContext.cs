using CxRestClient.IO;
using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static CxRestClient.CxSCARestContext;

namespace CxRestClient
{
    public class CxOneRestContext : CxCommonRestContext
    {
        private static readonly String LOGIN_URI_LEFT = "auth/realms";
        private static readonly String LOGIN_URI_RIGHT = "protocol/openid-connect/token";

        private static ILog _log = LogManager.GetLogger(typeof(CxOneRestContext));

        public override string LoginUrl => UrlUtils.MakeUrl(_InternalIAMUrl, LOGIN_URI_LEFT, Tenant, LOGIN_URI_RIGHT);
        public override string ApiUrl => _InternalApiUrl;

        private LoginToken _token;

        protected override LoginToken TokenImpl
        {
            get
            {
                if (_token == null)
                    _token = GetLoginToken(new Dictionary<string, string>()
                        {
                            {"client_id", "ast-app"},
                            {"grant_type", "refresh_token"},
                            {"refresh_token", ApiToken}
                        }
                    );

                ValidateToken(ref _token);
                return _token;
            }

        }

        internal String _InternalApiUrl { get; set; }
        internal String _InternalIAMUrl { get; set; }

        public String Tenant { get; internal set; }
        public String ApiToken { get; internal set; }

        public class CxOneRestContextBuilder : CxRestContextWithApiTokenBuilderBase<CxOneRestContextBuilder>
        {

            private String _tenant;
            public CxOneRestContextBuilder WithTenant(String tenant)
            {
                _tenant = tenant;
                return this;
            }

            private String _iamUrl;
            public CxOneRestContextBuilder WithIAMUrl(String url)
            {
                _iamUrl = url;
                return this;
            }

            internal override void Validate()
            {
                base.Validate();

                if (String.IsNullOrEmpty(_tenant))
                    throw new InvalidOperationException("Tenant was not provided is invalid.");

                if (String.IsNullOrEmpty(_iamUrl))
                    throw new InvalidOperationException("IAM URL was not specified.");

                if (!Uri.IsWellFormedUriString(_iamUrl, UriKind.Absolute))
                    throw new InvalidOperationException("IAM URL is invalid.");


            }

            public CxOneRestContext Build()
            {
                Validate();

                var timeoutSpan = new TimeSpan(0, 0, Timeout);

                HttpClientSingleton.Initialize(_validate, timeoutSpan);

                CxOneRestContext retVal = new CxOneRestContext()
                {
                    _InternalApiUrl = ApiUrl,
                    _InternalIAMUrl = _iamUrl,
                    ValidateSSL = _validate,
                    Timeout = timeoutSpan,
                    RetryLoop = RetryLoop,
                    Tenant = _tenant,
                    ApiToken = ApiToken
                };

                return retVal;
            }



        }
    }
}

