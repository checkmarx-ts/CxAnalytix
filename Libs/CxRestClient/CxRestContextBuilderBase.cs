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

        protected String User { get; private set; }
        public virtual T WithUsername(String username)
        {
            User = username;
            return this as T;
        }

        protected String Password { get; private set; }
        public virtual T WithPassword(String pass)
        {
            Password = pass;
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

        internal abstract void Validate();


    }
}
