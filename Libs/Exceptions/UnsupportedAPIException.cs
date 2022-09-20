using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Exceptions
{
    public class UnsupportedAPIException : Exception
    {
        public UnsupportedAPIException() : base("Unknown error")
        {
        }

        public UnsupportedAPIException(String msg) : base(msg)
        {
        }

        public UnsupportedAPIException(String msg, Exception ex) : base(msg, ex)
        {
        }

        public UnsupportedAPIException(Uri url, String version)
            : base($"API Endpoint {url} does not exist or does not support version {version}.")
        {
        }

        public UnsupportedAPIException(String url, String version)
            : base($"API Endpoint {url} does not exist or does not support version {version}.")
        {
        }

    }
}
