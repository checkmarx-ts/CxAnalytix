using System;
using NUnit.Framework;
using CxRestClient.IO;

namespace CxRestClient_Tests
{
    public class HttpClientSingleton_Tests
    {
        [Test]
        public void TestDoubleInitializeThrowsError ()
        {
            HttpClientSingleton.Initialize(true);

            try
            {
                HttpClientSingleton.Initialize(true);
            }
            catch (InvalidOperationException)
            {
                HttpClientSingleton.Clear();

                Assert.Pass();
                return;
            }

            HttpClientSingleton.Clear();
            Assert.Fail();
        }


        [Test]
        public void TestUninitializedAccessThrowsError ()
        {

            try
            {
                var h = HttpClientSingleton.GetClient();
            }
            catch (InvalidOperationException)
            {
                HttpClientSingleton.Clear();
                Assert.Pass();
                return;
            }


            HttpClientSingleton.Clear();
            Assert.Fail();
        }

        [Test]
        public void TestInitializedAccessGivesResult()
        {
            HttpClientSingleton.Initialize(true);

            var h = HttpClientSingleton.GetClient();
            bool result = h != null;
            HttpClientSingleton.Clear();
            Assert.True(result);
        }

    }
}
