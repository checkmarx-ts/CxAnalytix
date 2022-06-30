using CxRestClient.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.Interfaces
{
    internal interface ICxRestContext
    {

        LoginToken Token { get; }
        int RetryLoop { get; }
        TimeSpan Timeout { get; }
        void Reauthenticate();

    }
}
