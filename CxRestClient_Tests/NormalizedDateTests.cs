using CxRestClient;
using NUnit.Framework;
using System;

namespace CxRestClient_Tests
{
    public class NormalizedDateTests
    {
        [Test]
        public void EmptyStringExpectMinDate ()
        {
            Assert.True(CxSastScans.NormalizeDateParse("").Equals (DateTime.MinValue));
        }

        [Test]
        public void NullStringExpectMinDate()
        {
            Assert.True(CxSastScans.NormalizeDateParse(null).Equals(DateTime.MinValue));
        }

        [Test]
        public void IsoStringExpectIsoDate()
        {
            String dt = "2020-08-31T01:01:01Z";
            Assert.True(CxSastScans.NormalizeDateParse(dt).Equals(DateTime.Parse (dt)));
        }
    }
}
