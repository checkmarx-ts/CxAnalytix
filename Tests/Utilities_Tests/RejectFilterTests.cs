using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using CxAnalytix.Utilities.DictFilters;

namespace Utilities_Tests
{
	public class RejectFilterTests : BaseFilterTest
	{
		[Fact]
		public void NoFieldsDoesNotModifyDict()
		{
			var dict = Gen();
			var filter = DictionaryFilterFactory.CreateFilter<String, Object>(DictionaryFilterFactory.FilterModes.Reject, new List<String>() );

			var result = filter.Filter(dict);

			Assert.Equal(26, dict.Keys.Count);
		}

		[Fact]
		public void RejectsSpecified()
		{
			var dict = Gen();
			var filter = DictionaryFilterFactory.CreateFilter<String, Object>(DictionaryFilterFactory.FilterModes.Reject, _fields);

			var result = filter.Filter(dict);

			var set = new HashSet<String>(dict.Keys);
			set.UnionWith(_vowels);

			Assert.Equal(_vowels.Count, set.Count);
		}


	}
}
