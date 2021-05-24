using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Utilities.DictFilters
{
	internal class PassFilter<TKey, TValue> : DictionaryFilter<TKey, TValue>
	{

		internal PassFilter(IReadOnlyCollection<TKey> fields) : base(fields)
		{

		}

		public override IDictionary<TKey, TValue> Filter(IDictionary<TKey, TValue> src)
		{
			var keys = GetMatchingKeys(src.Keys);

			var set = new HashSet<TKey>(src.Keys);
			set.ExceptWith(GetMatchingKeys(src.Keys));

			foreach (var key in set)
				src.Remove(key);

			return src;
		}
	}
}
