using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.Utility
{
    public class IndexedCollection<T, TKEY> : AggregatedCollection<T>
    {
        private Dictionary<TKEY, T> Index { get; set; } = new();

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void SyncCombine(ICollection<T> other)
        {
            foreach (var elem in other)
                Add(elem);
        }

        public bool ContainsKey(TKEY key) => Index.ContainsKey(key);

        public T this[TKEY key]
        { 
            get => Index[key];
            set => Index[key] = value;
        }


    }
}
