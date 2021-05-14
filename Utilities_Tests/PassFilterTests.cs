using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using CxAnalytix.Utilities.DictFilters;

namespace Utilities_Tests
{
	public class PassFilterTests : BaseFilterTest
	{

		[Fact]
		public void NoFieldsRemovesAllFromDict()
		{
			var dict = Gen();
			var filter = DictionaryFilterFactory.CreateFilter<String, Object>(DictionaryFilterFactory.FilterModes.Pass, new List<String>());

			var result = filter.Filter(dict);

			Assert.Empty(result.Keys);
		}


		[Fact]
		public void PassesSpecified()
		{
			var dict = Gen();
			var filter = DictionaryFilterFactory.CreateFilter<String, Object>(DictionaryFilterFactory.FilterModes.Pass, _fields);

			var result = filter.Filter(dict);

			var set = new HashSet<String>(result.Keys);
			set.UnionWith(_fields);

			Assert.Equal(_fields.Count, set.Count);
		}

	}
}
