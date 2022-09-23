using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient
{
    public abstract class CxRestContextWithCredsBuilderBase<T> : CxRestContextBuilderBase<T> where T : CxRestContextWithCredsBuilderBase<T>
    {
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

        internal override void Validate()
        {
            base.Validate();

            if (String.IsNullOrEmpty(User))
                throw new InvalidOperationException("Username was not specified.");

            if (String.IsNullOrEmpty(Password))
                throw new InvalidOperationException("Password was not specified.");
        }
    }
}
