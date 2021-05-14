using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Utilities.DictFilters
{
	public abstract class DictionaryFilter<TKey, TValue>
	{
		protected HashSet<TKey> _fields;

		internal DictionaryFilter(IReadOnlyCollection<TKey> fields)
		{
			_fields = new HashSet<TKey>(fields);
		}

		protected ISet<TKey> GetMatchingKeys(ICollection<TKey> keys)
		{
			var retVal = new HashSet<TKey>(_fields);

			retVal.IntersectWith(keys);

			return retVal;
		}

		public abstract IDictionary<TKey, TValue> Filter(IDictionary<TKey, TValue> src);

	}
}
