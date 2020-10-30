using System;
using System.Collections.Generic;
using System.Text;
using Xunit;


namespace Test.TransformerLogic.ProjectResolver
{
    public class ProjectResolverTests
    {
        private CxAnalytix.TransformLogic.ProjectResolver pr;
        private static Dictionary<String, Action<CxAnalytix.TransformLogic.Data.ScanDescriptor, 
            CxAnalytix.TransformLogic.Transformer>> dummy = 
            new Dictionary<string, Action<CxAnalytix.TransformLogic.Data.ScanDescriptor, 
                CxAnalytix.TransformLogic.Transformer>>()
        {
                { "dummy", (d, t) => {} }
        };

        String Team1 = "1";
        String Team2 = "2";

        public ProjectResolverTests ()
        {
            CxAnalytix.TransformLogic.DataResolver dr = new CxAnalytix.TransformLogic.DataResolver();
            dr.addPreset(1, "Preset1");
            dr.addPreset(2, "Preset2");

            dr.addTeam(Team1, "\\Team1");
            dr.addTeam(Team2, "\\Team2");

            pr = dr.Resolve(null);
        }


        [Fact]
        public void CantAddProjectAfterResolve ()
        {
            bool shouldBeTrue = pr.AddProject(Team1, 1, 1, "Foo", "", new Dictionary<String, String>());

            var sr = pr.Resolve(dummy);

            bool shouldBeFalse = pr.AddProject(Team1, 1, 2, "Bar", "", new Dictionary<String, String>());

            Assert.True(shouldBeTrue && !shouldBeFalse);
        }

        [Fact]
        public void CantAddDuplicateProject ()
        {
            bool shouldBeTrue = pr.AddProject(Team1, 1, 1, "Foo", "", new Dictionary<String, String>());
            bool shouldBeFalse = pr.AddProject(Team1, 1, 1, "Bar", "", new Dictionary<String, String>());
            Assert.True(shouldBeTrue && !shouldBeFalse);
        }

        [Fact]
        public void CantAddProjectWithMissingPreset()
        {
            Assert.False(pr.AddProject (Team1, 100, 1, "Foo", "", new Dictionary<String, String>()) );
        }

        [Fact]
        public void CantAddProjetWithNullProjectName()
        {
            Assert.False(pr.AddProject(Team1, 1, 1, null, "", new Dictionary<String, String>()));
        }

        [Fact]
        public void CantAddProjectWithMissingTeam()
        {
            Assert.False(pr.AddProject(Guid.NewGuid().ToString(), 1, 1, "Foo", "", new Dictionary<String, String>()));
        }

        [Fact]
        public void CantAddProjectWithEmptyTeamGuid()
        {
            Assert.False(pr.AddProject(Guid.Empty.ToString(), 1, 1, "Foo", "", new Dictionary<String, String>()));
        }

        [Fact]
        public void MultiResolveReturnsSameObject ()
        {
            Assert.True(pr.Resolve(dummy) == pr.Resolve(dummy));
        }

    }
}
