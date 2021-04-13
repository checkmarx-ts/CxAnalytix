using CxAnalytix.TransformLogic.Persistence;
using System;
using System.IO;
using Xunit;


namespace TransformLogic_Tests
{
	public class CrawlStateTests : IClassFixture<CrawlStateTests.StorageFixture>
	{

		public class StorageFixture : IDisposable
		{

			String _testPath;

			public String TestPath { get => _testPath;}


			public StorageFixture ()
			{
				_testPath = Path.Combine(Path.GetTempPath (), "xunit"); 

			}
				

			public void Dispose()
			{
			}
		}


		public CrawlStateTests(CrawlStateTests.StorageFixture fixture)
		{
			this._fixture = fixture;

		}


		CrawlStateTests.StorageFixture _fixture;

		[Fact]
		public void TestAddProjectWhenEmpty()
		{
			var crawlState = new CrawlState(Path.Combine (_fixture.TestPath, "empty_test"));


			Assert.Equal(0, crawlState.ProjectCount);
		}

		[Fact]
		public void TestPersistWhenProjectAdded()
		{


		}

	}
}
