using LogCleaner;
using NUnit.Framework;
using System;
using System.IO;

namespace Tests
{
    public class Tests
    {
        private String _cwd;
        private String _oldFile, _newFile;

        private static String _fileSuffix = ".test.txt";
        private static String _outOfScopeFile = "dontdeleteme.txt";

        [SetUp]
        public void Setup()
        {
            _cwd = Directory.GetCurrentDirectory();
            _oldFile = Path.Join(_cwd, "old" + _fileSuffix);
            _newFile = Path.Join(_cwd, "new" + _fileSuffix);

            File.CreateText(_newFile).Close();
            File.CreateText(_outOfScopeFile).Close();
            File.CreateText(_oldFile).Close();
            File.SetLastWriteTime(_oldFile, DateTime.UnixEpoch);
        }

        [TearDown]
        public void Teardown()
        {
            if (File.Exists(_newFile))
                File.Delete(_newFile);

            if (File.Exists(_oldFile))
                File.Delete(_oldFile);

            if (File.Exists(_outOfScopeFile))
                File.Delete(_outOfScopeFile);
        }


        [Test]
        public void Canary()
        {
            Assert.Pass();
        }

        [Test]
        public void TestOldFileDeletedOnly()
        {
            Cleaner.CleanOldFiles(_cwd, "*" + _fileSuffix, 1);
            Assert.That(!File.Exists(_oldFile) && File.Exists(_newFile));
        }

        [Test]
        public void TestDeleteAllFilesInScopeWhen0Days()
        {
            Cleaner.CleanOldFiles(_cwd, "*" + _fileSuffix, 0);
            Assert.That(!File.Exists(_oldFile) && !File.Exists(_newFile) &&
                File.Exists(_outOfScopeFile));
        }

        [Test]
        public void TestNofilesDeletedWhenMaxDays()
        {
            Cleaner.CleanOldFiles(_cwd, "*" + _fileSuffix,
                Convert.ToInt32(DateTime.Now.Subtract(DateTime.UnixEpoch).TotalDays));
            Assert.That(File.Exists(_oldFile) && File.Exists(_newFile) &&
                File.Exists(_outOfScopeFile));
        }

        [Test]
        public void TestNofilesDeletedWhenDirectoryNull()
        {
            Cleaner.CleanOldFiles(null, "*" + _fileSuffix,
                Convert.ToInt32(DateTime.Now.Subtract(DateTime.UnixEpoch).TotalDays));
            Assert.That(File.Exists(_oldFile) && File.Exists(_newFile) &&
                File.Exists(_outOfScopeFile));
        }

        [Test]
        public void TestNofilesDeletedWhenSearchPatternNull()
        {
            Cleaner.CleanOldFiles(_cwd, null,
                Convert.ToInt32(DateTime.Now.Subtract(DateTime.UnixEpoch).TotalDays));
            Assert.That(File.Exists(_oldFile) && File.Exists(_newFile) &&
                File.Exists(_outOfScopeFile));
        }

        [Test]
        public void TestNofilesDeletedWhenDirectoryEmpty()
        {
            Cleaner.CleanOldFiles("", "*" + _fileSuffix,
                Convert.ToInt32(DateTime.Now.Subtract(DateTime.UnixEpoch).TotalDays));
            Assert.That(File.Exists(_oldFile) && File.Exists(_newFile) &&
                File.Exists(_outOfScopeFile));
        }

        [Test]
        public void TestNofilesDeletedWhenSearchPatternEmpty()
        {
            Cleaner.CleanOldFiles(_cwd, "",
                Convert.ToInt32(DateTime.Now.Subtract(DateTime.UnixEpoch).TotalDays));
            Assert.That(File.Exists(_oldFile) && File.Exists(_newFile) &&
                File.Exists(_outOfScopeFile));
        }


        [Test]
        public void TestNofilesDeletedWhenNegativeDays()
        {
            Cleaner.CleanOldFiles(_cwd, "",
                -Convert.ToInt32(DateTime.Now.Subtract(DateTime.UnixEpoch).TotalDays));
            Assert.That(File.Exists(_oldFile) && File.Exists(_newFile) &&
                File.Exists(_outOfScopeFile));
        }

    }
}