using CxRestClient.IO;
using CxRestClient.Utility;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace CxRestClient
{
    public class CxSCARestContext : CxSimpleRestContext
    {
        private static readonly String LOGIN_SUFFIX = "/identity/connect/token";

        public override string LoginUrl => UrlUtils.MakeUrl(_InternalLoginUrl, LOGIN_SUFFIX);
        public override string ApiUrl => _InternalApiUrl;
        internal String Tenant { get; set; }

        private String _InternalLoginUrl { get; set; }
        private String _InternalApiUrl { get; set; }

        internal CxSCARestContext()
        { }

        private LoginToken _token = null;
        protected override LoginToken TokenImpl
        {
            get
            {
                if (_token == null)
                    _token = GetLoginToken(new Dictionary<string, string>()
                        {
                            {"username", User},
                            {"password", Password},
                            {"acr_values", $"Tenant:{Tenant}"},
                            {"scope", "sca_api"},
                            {"client_id", "sca_resource_owner"},
                            {"grant_type", "password"}
                        }
                    );
                
                ValidateToken(ref _token);
                return _token;
            }
        }

        public class CxSCARestContextBuilder : CxRestContextWithCredsBuilderBase<CxSCARestContextBuilder>
        {
            private String _tenant;
            public CxSCARestContextBuilder WithTenant(String tenant)
            {
                _tenant = tenant;
                return this;
            }

            private String _loginUrl;
            public CxSCARestContextBuilder WithLoginUrl(String url)
            {
                _loginUrl = url;
                return this;
            }

            public CxSCARestContext Build()
            {
                Validate();

                var timeoutSpan = new TimeSpan(0, 0, Timeout);

                HttpClientSingleton.Initialize(_validate, timeoutSpan);

                CxSCARestContext retVal = new CxSCARestContext()
                {
                    User = User,
                    Password = Password,
                    _InternalApiUrl = ApiUrl,
                    _InternalLoginUrl = _loginUrl,
                    ValidateSSL = _validate,
                    Timeout = timeoutSpan,
                    RetryLoop = RetryLoop,
                    Tenant = _tenant
                };

                retVal.Json = new CxRestClientFactory("application/json", retVal);
                retVal.Xml = new CxRestClientFactory("application/xml", retVal);

                return retVal;
            }

            internal override void Validate()
            {
                base.Validate();

                if (String.IsNullOrEmpty(_tenant))
                    throw new InvalidOperationException("Tenant was not provided is invalid.");

                if (String.IsNullOrEmpty(_loginUrl))
                    throw new InvalidOperationException("Login URL was not specified.");

                if (!Uri.IsWellFormedUriString(_loginUrl, UriKind.Absolute))
                    throw new InvalidOperationException("Login URL is invalid.");


            }
        }

    }
}
