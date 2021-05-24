using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Utilities.DictFilters
{
	public class DictionaryFilterFactory
	{
		public enum FilterModes
		{
			None,
			Reject,
			Pass
		}

		private class NoFilter<TKey, TValue> : DictionaryFilter<TKey, TValue>
		{
			public NoFilter() : base (new List<TKey>() )
			{

			}

			public override IDictionary<TKey, TValue> Filter(IDictionary<TKey, TValue> src)
			{
				return src;
			}
		}

		public static DictionaryFilter<TKey, TValue> CreateFilter<TKey, TValue>(FilterModes mode, IReadOnlyCollection<TKey> fields)
		{

			switch (mode)
			{
				case FilterModes.Pass:
					return new PassFilter<TKey, TValue>(fields);

				case FilterModes.Reject:
					return new RejectFilter<TKey, TValue>(fields);
			}

			return new NoFilter<TKey, TValue>();
		}

	}
}
