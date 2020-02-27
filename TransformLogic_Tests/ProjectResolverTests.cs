using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;

namespace Test.TransformerLogic.ProjectResolver
{
    class ProjectResolverTests
    {
        private CxAnalytix.TransformLogic.ProjectResolver pr;
        private static Dictionary<String, Action<CxAnalytix.TransformLogic.Data.ScanDescriptor, 
            CxAnalytix.TransformLogic.Transformer>> dummy = 
            new Dictionary<string, Action<CxAnalytix.TransformLogic.Data.ScanDescriptor, 
                CxAnalytix.TransformLogic.Transformer>>()
        {
                { "dummy", (d, t) => {} }
        };

        Guid Team1 = Guid.NewGuid();
        Guid Team2 = Guid.NewGuid();

        [SetUp]
        public void SetupTest ()
        {
            CxAnalytix.TransformLogic.DataResolver dr = new CxAnalytix.TransformLogic.DataResolver();
            dr.addPreset(1, "Preset1");
            dr.addPreset(2, "Preset2");

            dr.addTeam(Team1, "\\Team1");
            dr.addTeam(Team2, "\\Team2");

            pr = dr.Resolve(null);
        }


        [Test]
        public void CantAddProjectAfterResolve ()
        {
            bool shouldBeTrue = pr.AddProject(Team1, 1, 1, "Foo", "");

            var sr = pr.Resolve(dummy);

            bool shouldBeFalse = pr.AddProject(Team1, 1, 2, "Bar", "");

            Assert.True(shouldBeTrue && !shouldBeFalse);
        }

        [Test]
        public void CantAddDuplicateProject ()
        {
            bool shouldBeTrue = pr.AddProject(Team1, 1, 1, "Foo", "");
            bool shouldBeFalse = pr.AddProject(Team1, 1, 1, "Bar", "");
            Assert.True(shouldBeTrue && !shouldBeFalse);
        }

        [Test]
        public void CantAddProjectWithMissingPreset()
        {
            Assert.False(pr.AddProject (Team1, 100, 1, "Foo", "") );
        }

        [Test]
        public void CantAddProjetWithNullProjectName()
        {
            Assert.False(pr.AddProject(Team1, 1, 1, null, ""));
        }

        [Test]
        public void CantAddProjectWithMissingTeam()
        {
            Assert.False(pr.AddProject(Guid.NewGuid(), 1, 1, "Foo", ""));
        }

        [Test]
        public void CantAddProjectWithEmptyTeamGuid()
        {
            Assert.False(pr.AddProject(Guid.Empty, 1, 1, "Foo", ""));
        }

        [Test]
        public void MultiResolveReturnsSameObject ()
        {
            Assert.That(pr.Resolve(dummy) == pr.Resolve(dummy));
        }

    }
}
