﻿using System;
using NUnit.Framework;
using CxRestClient.IO;

namespace CxRestClient_Tests
{
    [SingleThreaded]
    public class HttpClientSingleton_Tests
    {
        [Test]
        public void TestDoubleInitializeSuccess ()
        {
            HttpClientSingleton.Initialize(true, TimeSpan.FromSeconds(600));

            try
            {
                HttpClientSingleton.Initialize(true, TimeSpan.FromSeconds(600));
            }
            catch (InvalidOperationException)
            {
                HttpClientSingleton.Clear();
                Assert.Fail();
                return;
            }

            HttpClientSingleton.Clear();
            Assert.Pass();

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
            HttpClientSingleton.Initialize(true, TimeSpan.FromSeconds(600));

            var h = HttpClientSingleton.GetClient();
            bool result = h != null;
            HttpClientSingleton.Clear();
            Assert.True(result);
        }

    }
}
