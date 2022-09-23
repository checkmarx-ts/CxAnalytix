using CxRestClient.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient
{
    public abstract class CxCommonRestContext : CxRestContextBase
    {
        public CxCommonRestContext()
        {
            Json = new CxRestClientFactory("application/json", this);
            Xml = new CxRestClientFactory("application/xml", this);
            Any = new CxRestClientFactory("*/*", this);
        }
        public CxRestClientFactory Json { get; internal set; }
        public CxRestClientFactory Xml { get; internal set; }
        public CxRestClientFactory Any { get; internal set; }

    }
}
