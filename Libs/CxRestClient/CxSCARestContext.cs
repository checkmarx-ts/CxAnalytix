using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient
{
	public class CxSCARestContext : CxRestContextBase
	{
		public override void Reauthenticate()
		{
			throw new NotImplementedException();
		}

		internal CxSCARestContext()
		{ }


        public class CxSCARestContextBuilder : CxRestContextBuilderCommon<CxSCARestContextBuilder>
        {

            private bool _usSelected;

            public CxSCARestContextBuilder WithUSEnvironment()
			{
                _usSelected = true;
                return base.WithServiceURL("https://platform.checkmarx.net");
			}

            private bool _euSelected;
            public CxSCARestContextBuilder WithEUEnvironment()
            {
                _euSelected = true;
                return base.WithServiceURL("https://eu.platform.checkmarx.net");
            }

            private String _tenant;
            public CxSCARestContextBuilder WithTenant(String tenant)
			{
                _tenant = tenant;
                return this;
			}

            public CxSCARestContext Build()
            {
                Validate();
/*
                var timeoutSpan = new TimeSpan(0, 0, _timeout);

                HttpClientSingleton.Initialize(_validate, timeoutSpan);

                CxSASTRestContext retVal = new CxSASTRestContext()
                {
                    SastToken = GetSASTLoginToken(MakeUrl(_url, LOGIN_URI_SUFFIX), _user, _pass),
                    MNOToken = String.IsNullOrEmpty(_mnoUrl) ? new Nullable<LoginToken>() : GetMNOLoginToken(MakeUrl(_url, LOGIN_URI_SUFFIX), _user, _pass),
                    Url = _url,
                    MnoUrl = _mnoUrl,
                    ValidateSSL = _validate,
                    Timeout = timeoutSpan,
                    RetryLoop = _retryLoop
                };

                retVal.Json = new CxSASTClientFactory("application/json", retVal);
                retVal.Xml = new CxSASTClientFactory("application/xml", retVal);
*/
                return null;
            }

			internal override void Validate()
			{
				base.Validate();

                if (_usSelected && _euSelected)
                    throw new InvalidOperationException("Only one regional service endpoint can be selected.");
            }

            public override CxSCARestContextBuilder WithServiceURL(string url)
			{
                _usSelected = _euSelected = false;

				return base.WithServiceURL(url);
			}
		}

    }
}
