using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CxRestClient.Utility
{
    public class AggregatedCollection<T> : ICollection<T>
    {
        private List<T> _agg = new();

        public int Count => _agg.Count;

        public bool IsReadOnly => false;

        public virtual void Add(T item)
        {
            _agg.Add(item);
        }

        public void Clear()
        {
            _agg.Clear();
        }

        public bool Contains(T item)
        {
            return _agg.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _agg.CopyTo(array, arrayIndex);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _agg.GetEnumerator();
        }

        public bool Remove(T item)
        {
            return _agg.Remove(item);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _agg.GetEnumerator();
        }
    }
}
