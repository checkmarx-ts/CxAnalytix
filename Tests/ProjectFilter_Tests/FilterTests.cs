using CxAnalytix.Interfaces.Transform;
using ProjectFilter;
using Xunit;

namespace ProjectFilter_Tests
{
	public class FilterTests
	{

		[Fact]
		public void TestEmptyRegexMatchesEmptyString()
		{
			IProjectFilter f = new FilterImpl("", "");
			Assert.True(f.Matches("", "") );
		}

		[Fact]
		public void TestEmptyRegexMatchesAnyString()
		{
			IProjectFilter f = new FilterImpl("", "");
			Assert.True(f.Matches("foo", "bar"));
		}

		[Fact]
		public void TestNullTeamDoesNotMatch()
		{
			IProjectFilter f = new FilterImpl(".*", ".*");
			
			Assert.False(f.Matches(null, "foo"));
		}

		[Fact]
		public void TestNullProjectDoesNotMatch()
		{
			IProjectFilter f = new FilterImpl(".*", ".*");

			Assert.False(f.Matches("foo", null));
		}

		[Fact]
		public void TestBothNullDoesNotMatch()
		{
			IProjectFilter f = new FilterImpl(".*", ".*");

			Assert.False(f.Matches(null, null));
		}

		[Fact]
		public void TestNullRulesMatchNullProject()
		{
			IProjectFilter f = new FilterImpl(null, null);

			Assert.True(f.Matches("foo", null));
		}

		[Fact]
		public void TestNullRulesMatchNullTeam()
		{
			IProjectFilter f = new FilterImpl(null, null);

			Assert.True(f.Matches(null, "foo"));
		}

		[Fact]
		public void TestNullRulesMatchNullBoth()
		{
			IProjectFilter f = new FilterImpl(null, null);

			Assert.True(f.Matches(null, null));
		}

		[Fact]
		public void TestNullRulesMatchEmptyBoth()
		{
			IProjectFilter f = new FilterImpl(null, null);

			Assert.True(f.Matches("", ""));
		}

		[Fact]
		public void TestEmptyTeamRulesMatchTeam()
		{
			IProjectFilter f = new FilterImpl("", null);

			Assert.True(f.Matches("foo", ""));
		}

		[Fact]
		public void TestEmptyTeamRulesMatchTeamWithNullProject()
		{
			IProjectFilter f = new FilterImpl("", null);

			Assert.True(f.Matches("foo", null));
		}


		[Fact]
		public void TestEmptyProjectRulesMatchTeam()
		{
			IProjectFilter f = new FilterImpl(null, "");

			Assert.True(f.Matches("", "foo"));
		}

		[Fact]
		public void TestEmptyProjectRulesMatchProjectWithNullTeam()
		{
			IProjectFilter f = new FilterImpl(null, "");

			Assert.True(f.Matches(null, "foo"));
		}


		[Fact]
		public void TestMatch89StyleTeam ()
		{
			IProjectFilter f = new FilterImpl(@"\\usa$", null);

			Assert.True(f.Matches(@"\CxServer\Foo\bar\usa", null));
		}

		[Fact]
		public void TestNoMatch89StyleTeam()
		{
			IProjectFilter f = new FilterImpl(@"\\usa$", null);

			Assert.False(f.Matches(@"\CxServer\Foo\bar\uk", null));
		}

		[Fact]
		public void TestMatch89StyleProjectRightTeam()
		{
			IProjectFilter f = new FilterImpl(@"\\usa$", "master");

			Assert.True(f.Matches(@"\CxServer\Foo\bar\usa", "ORG_ProjectName_master"));
		}

		[Fact]
		public void TestNoMatch89StyleProjectWrongTeam()
		{
			IProjectFilter f = new FilterImpl(@"\\usa$", "master");

			Assert.False(f.Matches(@"\CxServer\Foo\bar\uk", "ORG_ProjectName_master"));
		}

		[Fact]
		public void TestNoMatch89StyleProjectWrongBranch()
		{
			IProjectFilter f = new FilterImpl(@"\\usa$", "master");

			Assert.False(f.Matches(@"\CxServer\Foo\bar\usa", "ORG_ProjectName_dev"));
		}

	}
}
