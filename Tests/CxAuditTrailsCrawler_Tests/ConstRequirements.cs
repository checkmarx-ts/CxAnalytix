using CxAnalytix.AuditTrails.Crawler;
using CxAnalytix.AuditTrails.Crawler.Config;
using System;
using Xunit;

namespace CxAuditTrailsCrawler_Tests
{
	public class ConstRequirements
	{
		[Fact]
		public void Canary()
		{
			Assert.True(true);
		}


		[Fact]
		void OptPropMustBeDeclaredMatchingEachTableNameConstField ()
		{
			Assert.True(FieldChecker.TypeHasMatchingTableNameProps(typeof(CxAuditTrailOpts<Object>)) );
		}

	}
}
