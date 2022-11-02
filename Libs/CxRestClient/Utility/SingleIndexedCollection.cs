using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static CxRestClient.CXONE.CxScanSummary;

namespace CxRestClient.Utility
{
    public abstract class SingleIndexedCollection<T, TKEY> : IndexedCollection<T, TKEY>
    {

        public abstract TKEY GetIndexKey(T item);

        public override void Add(T item)
        {
            base.Add(item);
            this[GetIndexKey(item)] = item;
        }

    }
}
