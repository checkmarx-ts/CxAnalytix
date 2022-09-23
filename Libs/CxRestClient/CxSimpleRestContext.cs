using CxRestClient.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient
{
    public abstract class CxSimpleRestContext : CxCommonRestContext
    {

        public CxSimpleRestContext() : base()
        {
        }


        internal String User { get; set; }
        internal String Password { get; set; }

    }
}
