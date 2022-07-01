using CxRestClient.Utility;
using NUnit.Framework;
using System;

namespace CxRestClient_Tests
{
    public class NormalizedDateTests
    {
        [Test]
        public void EmptyStringExpectMinDate ()
        {
            Assert.True(JsonUtils.NormalizeDateParse("").Equals (DateTime.MinValue));
        }

        [Test]
        public void NullStringExpectMinDate()
        {
            Assert.True(JsonUtils.NormalizeDateParse(null).Equals(DateTime.MinValue));
        }

        [Test]
        public void IsoStringExpectIsoDate()
        {
            String dt = "2020-08-31T01:01:01Z";
            Assert.True(JsonUtils.NormalizeDateParse(dt).Equals(DateTime.Parse (dt)));
        }
    }
}
