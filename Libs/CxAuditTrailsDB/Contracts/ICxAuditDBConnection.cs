using System;
using System.Collections.Generic;
using System.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.CxAuditTrails.DB.Contracts
{
    public interface ICxAuditDBConnection
    {
        String ConnectionString { get; }

    }
}
