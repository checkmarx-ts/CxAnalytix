using System;
using NUnit.Framework;
using CxRestClient;


namespace CxRestClient_Tests
{
	public class CxSASTRestContextBuilderTests
	{

		[Test]
		public void ValidateWithInvalidMnoServiceUrlFails()
		{
			try
			{
				new CxSASTRestContext.CxSASTRestContextBuilder()
					.WithUsername("user")
					.WithPassword("pass")
					.WithMNOServiceURL("not_a_url")
					.WithApiURL("http://www.foo.com")
					.Validate();
			}
			catch (Exception)
			{
				Assert.True(true);
				return;
			}


			Assert.True(false);
		}

	}
}
