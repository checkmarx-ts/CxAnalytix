using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient
{
    public abstract class CxRestContextBuilderBase<T> where T : CxRestContextBuilderBase<T>
    {

        protected CxRestContextBuilderBase()
		{
            Timeout = 600;
		}

        protected bool _validate = true;
        public virtual T WithSSLValidate(bool validate)
        {
            _validate = validate;
            return this as T;
        }

        protected int Timeout { get; private set; }
        public virtual T WithOpTimeout(int seconds)
        {
            Timeout = seconds;
            return this as T;
        }

        protected int RetryLoop { get; private set; }
        public virtual T WithRetryLoop(int loopCount)
        {
            RetryLoop = loopCount;
            return this as T;
        }

        protected String ApiUrl { get; private set; }
        public virtual T WithApiURL(String url)
        {
            ApiUrl = url;
            return this as T;
        }

        internal virtual void Validate()
        {
            if (RetryLoop < 0)
                throw new InvalidOperationException("Retry loop can't be < 0.");

            if (Timeout < 0)
                throw new InvalidOperationException("Timeout can't be < 0.");

            if (String.IsNullOrEmpty(ApiUrl))
                throw new InvalidOperationException("API URL was not specified.");

            if (!Uri.IsWellFormedUriString(ApiUrl, UriKind.Absolute))
                throw new InvalidOperationException("API URL is invalid.");

        }


    }
}
