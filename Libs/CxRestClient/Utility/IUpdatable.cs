using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.Utility
{
    public interface IUpdatable<T>
    {
        void Update(T other);
    }
}
