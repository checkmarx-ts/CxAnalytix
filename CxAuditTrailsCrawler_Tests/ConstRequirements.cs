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
			var constFields = typeof(CxAuditTrailTableNameConsts).GetFields();
			var optType = typeof(CxAuditTrailOpts<Object>);

			bool missingField = false;

			foreach (var field in constFields)
			{
				missingField = optType.GetProperty(field.Name) == null;

				if (missingField)
					break;
			}

			Assert.True(!missingField);
		}

	}
}
