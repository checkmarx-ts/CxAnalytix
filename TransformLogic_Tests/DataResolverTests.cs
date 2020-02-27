using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.TransformerLogic.DataResolver
{

    public class DataResolverTests
    {
        CxAnalytix.TransformLogic.DataResolver d;

        [SetUp]
        public void SetupTest ()
        {
            d = new CxAnalytix.TransformLogic.DataResolver();
        }

        [Test]
        public void CantAddTeamPostResolve ()
        {
            bool shouldBeTrue = d.addTeam(Guid.NewGuid(), "Foo");

            var r = d.Resolve(null);

            bool shouldBeFalse = d.addTeam(Guid.NewGuid(), "Bar");

            Assert.True(shouldBeTrue && !shouldBeFalse);


        }

        [Test]
        public void CantAddPresetPostResolve()
        {
            bool shouldBeTrue = d.addPreset(1, "Foo");

            var r = d.Resolve(null);

            bool shouldBeFalse = d.addPreset(1, "Bar");

            Assert.True(shouldBeTrue && !shouldBeFalse);

        }


        [Test]
        public void CantAddPresetWithNullName ()
        {
            Assert.False(d.addPreset(1, null) );
        }

        [Test]
        public void CantAddTeamWithNullName ()
        {
            Assert.False(d.addTeam(Guid.NewGuid (), null));
        }

        [Test]
        public void CantAddTeamWithEmptyGuid ()
        {
            Assert.False(d.addTeam(Guid.Empty, String.Empty));
        }

        [Test]
        public void MultiResolveReturnsSameObject ()
        {
            Assert.That(d.Resolve(null) == d.Resolve(null));
        }
    }
}
