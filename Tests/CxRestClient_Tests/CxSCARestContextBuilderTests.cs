using System;
using NUnit.Framework;
using CxRestClient;


namespace CxRestClient_Tests
{
	public class CxSCARestContextBuilderTests
	{


		[Test]
		public void ValidateWithoutEUorUSSelectionFails()
		{
			try
			{
				new CxSCARestContext.CxSCARestContextBuilder()
					.WithUsername("user")
					.WithPassword("pass")
					.Validate();
			}
			catch (Exception)
			{
				Assert.Pass();
				return;

			}

			Assert.Fail();
		}

		[Test]
		public void ValidateWithEUandUSSelectionFails()
		{
			try
			{
				new CxSCARestContext.CxSCARestContextBuilder()
					.WithUsername("user")
					.WithPassword("pass")
					.WithTenant("tenant")
					.WithEUEnvironment()
					.WithUSEnvironment()
					.Validate();
			}
			catch (Exception)
			{
				Assert.Pass();
				return;

			}

			Assert.Fail();
		}

		[Test]
		public void ValidateExplicitServiceURLResetsRegionalSelection()
		{
			try
			{
				new CxSCARestContext.CxSCARestContextBuilder()
					.WithUsername("user")
					.WithPassword("pass")
					.WithTenant("tenant")
					.WithEUEnvironment()
					.WithUSEnvironment()
					.WithServiceURL("http://foo.com:8080")
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
