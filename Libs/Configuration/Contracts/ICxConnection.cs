using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Contracts
{
    public interface ICxConnection
    {
        String URL { get; }
        String MNOUrl { get; }
        int TimeoutSeconds { get; }
        bool ValidateCertificates { get; }
        int RetryLoop { get; }
    }
}
