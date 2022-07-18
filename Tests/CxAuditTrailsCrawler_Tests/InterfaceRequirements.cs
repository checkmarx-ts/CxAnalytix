using CxAnalytix.Interfaces.Audit;
using System;
using Xunit;

namespace CxAuditTrailsCrawler_Tests
{
	public class InterfaceRequirements
	{

		[Fact]
		public void Canary()
		{
			Assert.True(true);
		}


		[Fact]
		void IAuditTrailCrawlerMustHaveMethodMatchingTableNames ()
		{
			Assert.True(FieldChecker.TypeHasMatchingTableNameMethods(typeof(IAuditTrailCrawler)));
		}
	}
}
