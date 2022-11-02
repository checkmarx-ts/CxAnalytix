using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient
{
    public abstract class CxRestContextWithApiTokenBuilderBase<T>: CxRestContextBuilderBase<T> where T : CxRestContextWithApiTokenBuilderBase<T>
    {

        protected String ApiToken { get; private set; }
        public virtual T WithApiToken(String token)
        {
            ApiToken = token;
            return this as T;
        }


        internal override void Validate()
        {
            base.Validate();

            if (String.IsNullOrEmpty(ApiToken))
                throw new InvalidOperationException("Invalid API token.");
        }
    }
}
