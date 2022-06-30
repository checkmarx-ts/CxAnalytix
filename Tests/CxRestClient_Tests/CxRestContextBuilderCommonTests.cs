using System;
using NUnit.Framework;
using CxRestClient;

namespace CxRestClient_Tests
{

	[TestFixture(typeof(CxSASTRestContext.CxSASTRestContextBuilder))]
	[TestFixture(typeof(CxSCARestContext.CxSCARestContextBuilder))]
	public class CxRestContextBuilderCommonTests<T> where T : CxRestContextBuilderBase<T>, new()
	{

		[Test]
		public void ValidateWithoutServiceUrlFails()
		{
			try
			{
				new T()
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
		public void ValidateWithoutUserFails()
		{
			try
			{
				new T()
					.WithPassword("pass")
					.WithApiURL("http://www.foo.com")
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
		public void ValidateWithoutPasswordFails()
		{
			try
			{
				new T()
					.WithUsername("user")
					.WithApiURL("http://www.foo.com")
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
		public void ValidateWithServiceUrlSuccess()
		{
			try
			{
				new T()
					.WithUsername("user")
					.WithPassword("pass")
					.WithApiURL("http://www.foo.com")
					.Validate();
			}
			catch (Exception)
			{
				Assert.Fail();
				return;
			}


			Assert.Pass();
		}


		[Test]
		public void ValidateWithInvalidServiceUrlFails()
		{
			try
			{
				new T()
					.WithUsername("user")
					.WithPassword("pass")
					.WithApiURL("not_a_url")
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
		public void ValidateRetryLoopZeroSuccess()
		{
			try
			{
				new T()
					.WithUsername("user")
					.WithPassword("pass")
					.WithApiURL("http://www.foo.com")
					.WithRetryLoop(0)
					.Validate();
			}
			catch (Exception)
			{
				Assert.Fail();
				return;
			}


			Assert.Pass();
		}

		[Test]
		public void ValidateRetryLoopGtZeroSuccess()
		{
			try
			{
				new T()
					.WithUsername("user")
					.WithPassword("pass")
					.WithApiURL("http://www.foo.com")
					.WithRetryLoop(1)
					.Validate();
			}
			catch (Exception)
			{
				Assert.Fail();
				return;
			}


			Assert.Pass();
		}

		[Test]
		public void ValidateRetryLoopLtZeroFails()
		{
			try
			{
				new T()
					.WithUsername("user")
					.WithPassword("pass")
					.WithApiURL("http://www.foo.com")
					.WithRetryLoop(-1)
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
		public void ValidateTimeoutZeroSuccess()
		{
			try
			{
				new T()
					.WithUsername("user")
					.WithPassword("pass")
					.WithApiURL("http://www.foo.com")
					.WithOpTimeout(0)
					.Validate();
			}
			catch (Exception)
			{
				Assert.Fail();
				return;
			}


			Assert.Pass();
		}
		[Test]
		public void ValidateTimeoutGtZeroSuccess()
		{
			try
			{
				new T()
					.WithUsername("user")
					.WithPassword("pass")
					.WithApiURL("http://www.foo.com")
					.WithOpTimeout(1)
					.Validate();
			}
			catch (Exception)
			{
				Assert.Fail();
				return;
			}


			Assert.Pass();
		}

		[Test]
		public void ValidateTimeoutLtZeroFails()
		{
			try
			{
				new T()
					.WithUsername("user")
					.WithPassword("pass")
					.WithApiURL("http://www.foo.com")
					.WithOpTimeout(-1)
					.Validate();
			}
			catch (Exception)
			{
				Assert.Pass();
				return;
			}


			Assert.Fail();
		}

	}
}
