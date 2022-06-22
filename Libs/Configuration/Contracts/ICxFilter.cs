using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxAnalytix.Configuration.Contracts
{
    public interface ICxFilter
    {
        String TeamRegex { get; }
        String ProjectRegex { get; }
    }
}
