using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Test.TransformerLogic.ScanResolver
{
    class ScanResolverTests
    {

        CxAnalytics.TransformLogic.ScanResolver sr;

        Guid Team1 = Guid.NewGuid();
        Guid Team2 = Guid.NewGuid();

        MemoryStream stateStorage;
        StreamWriter stateWriter;

        [SetUp]
        public void SetupTest ()
        {
            stateStorage = new MemoryStream();
            stateWriter = new StreamWriter(stateStorage);

            var pr = makeDefaultProjectResolver(makeDefaultDataResolver());

            sr = pr.Resolve();
        }


        private CxAnalytics.TransformLogic.DataResolver makeDefaultDataResolver ()
        {
            CxAnalytics.TransformLogic.DataResolver d = new CxAnalytics.TransformLogic.DataResolver();
            d.addPreset(1, "Preset1");
            d.addPreset(2, "Preset2");

            d.addTeam(Team1, "\\Team1");
            d.addTeam(Team2, "\\Team2");

            return d;
        }

        private CxAnalytics.TransformLogic.ProjectResolver 
            makeDefaultProjectResolver (CxAnalytics.TransformLogic.DataResolver fromDR)
        {
            var retVal = fromDR.Resolve(null, stateWriter);

            populateProjectResolver(retVal);

            return retVal;
        }

        private void populateProjectResolver (CxAnalytics.TransformLogic.ProjectResolver pr)
        {
            pr.AddProject(Team1, 1, 1, "\\Team1\\ProjectId1", "");
            pr.AddProject(Team1, 1, 2, "\\Team1\\ProjectId2", "");

            pr.AddProject(Team2, 1, 3, "\\Team2\\ProjectId3", "");
            pr.AddProject(Team2, 2, 4, "\\Team2\\ProjectId4", "");

        }


        [Test]
        public void ExpectedNumberOfScansAndProjectsWithState()
        {
            DateTime check = DateTime.Now;

            // Our current state has all projects checked at epoch because of no
            // previous state.  This means all scans should be selected when added.

            // Add a scan that completed yesterday at this time.
            bool Scan1 = sr.addScan(1, "Full", "SAST", "ScanId1", check.AddDays(-1));

            // Add a scan that will have completed after the check
            bool Scan2 = sr.addScan(1, "Full", "SAST", "ScanId2", check.AddDays(1));

            // A scan completed "now"
            bool Scan3 = sr.addScan(4, "Full", "SAST", "ScanId3", check);

            // Resolve should give us back scans, and now there is state written in
            // this.stateStorage
            var scans = sr.Resolve(check);

            // Now do the same thing with state of previous check
            MemoryStream junk = new MemoryStream();
            stateStorage.Seek(0, SeekOrigin.Begin);

            StreamReader reader = new StreamReader(stateStorage);

            var pr = makeDefaultDataResolver().Resolve(reader, new StreamWriter(junk));

            // Fill the resolver with the default projects
            populateProjectResolver(pr);

            var nextsr = pr.Resolve();

            // Similate the next check happening an hour from now

            // Add the same scans, the one after the check date (1 day added) should be found
            Scan1 = nextsr.addScan(1, "Full", "SAST", "ScanId1", check.AddDays(-1));
            Scan2 = nextsr.addScan(1, "Full", "SAST", "ScanId2", check.AddDays(1));
            Scan3 = nextsr.addScan(4, "Full", "SAST", "ScanId3", check);

            var nextscans = nextsr.Resolve(check.AddHours (1.0) );



            Assert.True (nextsr.ResolvedScanCount == 1 && nextsr.ResolvedProjectCount == 1);
        }


        [Test]
        public void ScanCountNullUntilAfterResolve()
        {
            DateTime check = DateTime.Now;

            bool Scan1 = sr.addScan(1, "Full", "SAST", "ScanId1", DateTime.Now.AddDays(-1));

            bool Scan2 = sr.addScan(1, "Full", "SAST", "ScanId2", DateTime.Now.AddDays(1));

            bool Scan3 = sr.addScan(4, "Full", "SAST", "ScanId3", check);

            bool isNull = sr.ResolvedScanCount == null;

            var scans = sr.Resolve(check);
            
            bool isNotNull = sr.ResolvedScanCount != null;

            Assert.True(isNull && isNotNull);
        }

        [Test]
        public void ProjectCountNullUntilAfterResolve()
        {
            DateTime check = DateTime.Now;

            bool Scan1 = sr.addScan(1, "Full", "SAST", "ScanId1", DateTime.Now.AddDays(-1));

            bool Scan2 = sr.addScan(1, "Full", "SAST", "ScanId2", DateTime.Now.AddDays(1));

            bool Scan3 = sr.addScan(4, "Full", "SAST", "ScanId3", check);

            bool isNull = sr.ResolvedProjectCount == null;

            var scans = sr.Resolve(check);

            bool isNotNull = sr.ResolvedProjectCount != null;

            Assert.True(isNull && isNotNull);
        }

        [Test]
        public void CantAddScanWithInvalidProjectId ()
        {
            Assert.False (sr.addScan(0, "Full", "SAST", "ScanId1", DateTime.Now.AddDays(-1)));
        }

        [Test]
        public void CantAddDuplicateScanId()
        {
            bool shouldBeTrue = sr.addScan(1, "Full", "SAST", "ScanId1", DateTime.Now.AddDays(-1));
            bool shouldBeFalse = sr.addScan(1, "Full", "SAST", "ScanId1", DateTime.Now.AddDays(1));

            Assert.True(shouldBeTrue && !shouldBeFalse);
        }

        [Test]
        public void CantAddScanWithNullScanType()
        {
            Assert.False(sr.addScan(1, null, "SAST", "ScanId1", DateTime.Now.AddDays(-1)));
        }

        [Test]
        public void CantAddScanWithNullScanProduct()
        {
            Assert.False(sr.addScan(1, "Full", null, "ScanId1", DateTime.Now.AddDays(-1)));
        }

        [Test]
        public void CantAddScanWithNullScanId()
        {
            Assert.False(sr.addScan(1, "Full", "SCA", null, DateTime.Now.AddDays(-1)));
        }

        [Test]
        public void CantAddScanAfterResolve()
        {
            var junk = sr.Resolve(DateTime.Now);

            Assert.False(sr.addScan(1, "Full", "SCA", null, DateTime.Now.AddDays(-1)));
        }

        [Test]
        public void NoScansReturnedWhenFinishTimesAreEqualToCheck()
        {

            bool Scan1 = sr.addScan(1, "Full", "SAST", "ScanId1", DateTime.MinValue);

            bool Scan2 = sr.addScan(1, "Full", "SAST", "ScanId2", DateTime.MinValue);

            bool Scan3 = sr.addScan(4, "Full", "SAST", "ScanId3", DateTime.MinValue);

            var scans = sr.Resolve(DateTime.Now);

            Assert.That(sr.ResolvedScanCount == 0);
        }

    }
}
