using CxRestClient.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient
{
    public abstract class CxSimpleRestContext : CxRestContextBase
    {

        public CxSimpleRestContext()
        {
            Json = new CxRestClientFactory("application/json", this);
            Xml = new CxRestClientFactory("application/xml", this);
            Any = new CxRestClientFactory("*/*", this);
        }

        public CxRestClientFactory Json { get; internal set; }
        public CxRestClientFactory Xml { get; internal set; }
        public CxRestClientFactory Any { get; internal set; }

        internal String User { get; set; }
        internal String Password { get; set; }

    }
}
