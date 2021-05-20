using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using CxAnalytix.Extensions;


namespace Extensions_Test
{
	public class StringTruncateTests
	{
		[Test]
		public void ReduceSizeOfString()
		{
			String s = "12345";

			Assert.That(s.Truncate(2).Length == 2);
		}


		[Test]
		public void SmallerStringReturnsExactSameString()
		{
			String s = "12345";

			Assert.That(s.Truncate(100).CompareTo(s) == 0);
		}


		[Test]
		public void NullStringThrowsException()
		{

			try
			{
				String s = null;

				s.Truncate(100);

			}
			catch (Exception)
			{
				Assert.True(true);
				return;

			}

			Assert.True(false);
		}


		[Test]
		public void EmptyStringReturnsSameString()
		{
			String s = "";

			Assert.That(s.Truncate(100).CompareTo(s) == 0);
		}



	}
}
