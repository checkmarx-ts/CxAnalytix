using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.CXONE.Common
{
    public interface IMultiKeyed<T>
    {
        T GetKeys(); 
    }
}
