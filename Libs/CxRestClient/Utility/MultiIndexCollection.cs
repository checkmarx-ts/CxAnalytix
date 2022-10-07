using CxRestClient.CXONE.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace CxRestClient.Utility
{
    public abstract class MultiIndexCollection<T, TKEY> : ICollection<T> where T : IMultiKeyed<List<TKEY>>
    {
        private IndexedCollection<List<T>, TKEY> _collection = new();

        public int Count => _collection.Count;

        public bool IsReadOnly => false;

        public abstract List<TKEY> GetIndexKeys(T item);

        public List<T> this[TKEY key] => _collection[key];


        public void Add(T item)
        {
            foreach(TKEY key in GetIndexKeys(item))
                if (_collection.ContainsKey(key))
                    _collection[key].Add(item);
                else
                    _collection[key] = new List<T>() { item };
        }


        public bool ContainsKey(TKEY key) => _collection.ContainsKey(key);

        public void Clear() => throw new NotImplementedException();

        public bool Contains(T item) => throw new NotImplementedException();

        public void CopyTo(T[] array, int arrayIndex) => throw new NotImplementedException();

        public bool Remove(T item) => throw new NotImplementedException();

        public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();
        
        IEnumerator IEnumerable.GetEnumerator() => throw new NotImplementedException();
    }
}
