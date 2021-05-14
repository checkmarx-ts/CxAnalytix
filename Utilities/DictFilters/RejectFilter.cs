using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Utilities.DictFilters
{
	internal class RejectFilter<TKey, TValue> : DictionaryFilter<TKey, TValue>
	{

		internal RejectFilter(IReadOnlyCollection<TKey> fields) : base(fields)
		{

		}


		public override IDictionary<TKey, TValue> Filter(IDictionary<TKey, TValue> src)
		{
			var keys = GetMatchingKeys(src.Keys);

			foreach (var key in keys)
				src.Remove(key);

			return src;
		}
	}
}
