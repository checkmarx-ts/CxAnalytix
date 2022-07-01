using System;
using NUnit.Framework;
using CxRestClient;


namespace CxRestClient_Tests
{
	public class CxSCARestContextBuilderTests
	{

		[Test]
		public void ValidateExplicitServiceURLResetsRegionalSelection()
		{
			try
			{
				new CxSCARestContext.CxSCARestContextBuilder()
					.WithUsername("user")
					.WithPassword("pass")
					.WithTenant("tenant")
					.WithApiURL("http://foo.com:8080")
					.Validate();
			}
			catch (Exception)
			{
				Assert.Fail();
				return;

			}

			Assert.Pass();
		}

	}
}
