using Xunit;
using System;
using System.Collections.Generic;
using System.Text;

namespace Test.TransformerLogic.DataResolver
{

    public class DataResolverTests
    {
        CxAnalytix.TransformLogic.DataResolver d;

        public DataResolverTests ()
        {
            d = new CxAnalytix.TransformLogic.DataResolver();
        }

        [Fact]
        public void CantAddTeamPostResolve ()
        {
            bool shouldBeTrue = d.addTeam(Guid.NewGuid().ToString (), "Foo");

            var r = d.Resolve(null);

            bool shouldBeFalse = d.addTeam(Guid.NewGuid().ToString(), "Bar");

            Assert.True(shouldBeTrue && !shouldBeFalse);


        }

        [Fact]
        public void CantAddPresetPostResolve()
        {
            bool shouldBeTrue = d.addPreset(1, "Foo");

            var r = d.Resolve(null);

            bool shouldBeFalse = d.addPreset(1, "Bar");

            Assert.True(shouldBeTrue && !shouldBeFalse);

        }


        [Fact]
        public void CantAddPresetWithNullName ()
        {
            Assert.False(d.addPreset(1, null) );
        }

        [Fact]
        public void CantAddTeamWithNullName ()
        {
            Assert.False(d.addTeam(Guid.NewGuid ().ToString(), null));
        }

        [Fact]
        public void CantAddTeamWithEmptyGuid ()
        {
            Assert.False(d.addTeam(String.Empty, String.Empty));
        }

        [Fact]
        public void MultiResolveReturnsSameObject ()
        {
            Assert.True(d.Resolve(null) == d.Resolve(null));
        }
    }
}
