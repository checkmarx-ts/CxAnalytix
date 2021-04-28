using CxAnalytix.TransformLogic.Data;
using CxAnalytix.TransformLogic.Persistence;
using System;
using System.Collections.Generic;
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

				if (!Directory.Exists(_testPath))
					Directory.CreateDirectory(_testPath);
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
		public void NoScansWithoutScansAdded()
		{
			var crawlState = new CrawlState(_fixture.TestPath);

			List<ProjectDescriptor> list1 = new List<ProjectDescriptor>
			{
				new ProjectDescriptor ()
				{
					ProjectId = 1
					, TeamId="1"
				}
				, new ProjectDescriptor ()
				{
					ProjectId = 2
					, TeamId= "2"
				}
			};

			crawlState.ConfirmProjects(list1);

			var scans = crawlState.GetScansForProject(1);

			Assert.False(scans.GetEnumerator().MoveNext () ) ;
		}

		[Fact]
		public void CannotAskForScansWithoutProjectsAdded()
		{
			var crawlState = new CrawlState(_fixture.TestPath);

			try
			{
				crawlState.GetScansForProject(1);
			}
			catch (KeyNotFoundException)
			{
				Assert.True(true);
				return;
			}
			
			Assert.True(false);
		}



		[Fact]
		public void CannotConfirmProjectsTwice()
		{

			var crawlState = new CrawlState(_fixture.TestPath);

			List<ProjectDescriptor> list1 = new List<ProjectDescriptor>
			{
				new ProjectDescriptor ()
				{
					ProjectId = 1
					, TeamId="1"
				}
				, new ProjectDescriptor ()
				{
					ProjectId = 2
					, TeamId= "2"
				}
			};


			List<ProjectDescriptor> list2 = new List<ProjectDescriptor>(list1);
			list2.Add(new ProjectDescriptor()
			{
				ProjectId = 3,
				TeamId = "3"});


			crawlState.ConfirmProjects(list1);


			try
			{
				crawlState.ConfirmProjects(list2);

			}
			catch (InvalidOperationException)
			{
				Assert.True(true);
				return;
			}

			Assert.True(false);
		}

		/*
		 * 
		 * ask for scans with invalid project id
		 * 
		 * add scans for invalid project id
		 * 
		 * 
		 * add scan before confirmed
		 * 
		 * 
		 */

	}
}
