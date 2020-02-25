using CxRestClient;
using NUnit.Framework;
using System;

namespace CxRestClient_Tests
{
    public class DateTimeConversionTests

    {
        [Test]
        public void TestZeroResultsInJan11970Local()
        {
            var localEpochDate = new DateTime(1970, 1, 1, 0, 0, 0, 
                DateTimeKind.Local);

            var convertedDate = JsonUtils.LocalEpochTimeToDateTime(0);

            Assert.AreEqual(localEpochDate, convertedDate);
        }

        [Test]
        public void TestZeroResultsInJan11970Utc()
        {
            var localEpochDate = new DateTime(1970, 1, 1, 0, 0, 0,
                DateTimeKind.Utc);

            var convertedDate = JsonUtils.UtcEpochTimeToDateTime(0);

            Assert.AreEqual(localEpochDate, convertedDate);
        }
    }
}