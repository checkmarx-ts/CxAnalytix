using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text;

namespace Utilities_Tests
{
	public class BaseFilterTest
	{
		protected List<String> _fields = new List<string> 
			{ "B", "C", "D", "F", "G", "H", "J", "K", "L", "M", "N", "P", "Q", "R", "S", "T", "V", "W", "X", "Y", "Z" };

		protected List<String> _vowels = new List<string>
			{ "A", "E", "I", "O", "U" };

		protected Dictionary<String, Object> Gen ()
		{
			var retVal = new Dictionary<String, Object>();

			foreach (var l in _fields)
				retVal.Add(l, l);

			foreach (var l in _vowels)
				retVal.Add(l, l);

			return retVal;
		}
	}
}
