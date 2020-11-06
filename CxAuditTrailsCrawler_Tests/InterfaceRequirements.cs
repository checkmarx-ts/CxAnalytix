using CxAnalytix.Interfaces.Audit;
using System;
using Xunit;

namespace CxAuditTrailsCrawler_Tests
{
	public class InterfaceRequirements
	{

		[Fact]
		void IAuditTrailCrawlerMustHaveMethodMatchingTableNames ()
		{
			Assert.True(FieldChecker.TypeHasMatchingTableNameProps(typeof(IAuditTrailCrawler)));
		}
	}
}
