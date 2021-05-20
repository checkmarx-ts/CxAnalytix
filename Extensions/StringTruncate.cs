using System;
using System.Collections.Generic;
using System.Text;

namespace CxAnalytix.Extensions
{
	public static class StringTruncate
	{

		public static String Truncate(this String str, int maxSize)
		{
			if (str == null)
				throw new InvalidOperationException("Cannot truncate a null string.");

			if (str.Length == 0 || str.Length <= maxSize)
				return str;

			return str.Substring(0, maxSize);
		}

	}
}
