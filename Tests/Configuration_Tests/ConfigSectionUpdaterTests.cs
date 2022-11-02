using CxAnalytix.Configuration.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Configuration_Tests
{
    public class ConfigSectionUpdaterTests
    {
        private static String _configFile = Path.Combine(Directory.GetCurrentDirectory(), "cxanalytix.config");

        private String CopyForTest(String filename)
        {
            var dest = Path.Combine(Directory.GetCurrentDirectory(), filename);
            var src = Path.Combine("../../..", filename);

            if (File.Exists(dest) )
                File.Delete(dest);

            File.Copy(src, dest);

            return dest;
        }



        [Fact]
        public void Canary()
        {
            Assert.True(true);
        }

        [Fact]
        public void ValidForValidConfig()
        {
            Assert.True(ConfigSectionValidator.IsValid(CopyForTest("valid_case.config")));
        }

        [Fact]
        public void IgnoreExtraItems()
        {
            Assert.True(ConfigSectionValidator.IsValid(CopyForTest("ignore_extra_items_case.config")));
        }

        [Fact]
        public void InvalidForSingleRemovable()
        {
            Assert.False(ConfigSectionValidator.IsValid(CopyForTest("single_removable_section_case.config") ));
        }

        [Fact]
        public void InvalidForMultiRemovable()
        {
            Assert.False(ConfigSectionValidator.IsValid(CopyForTest("multi_removable_section_case.config")));
        }

        [Fact]
        public void InvalidForSingleMissing()
        {
            Assert.False(ConfigSectionValidator.IsValid(CopyForTest("single_removable_section_case.config")));
        }

        [Fact]
        public void InvalidForMultiMissing()
        {
            Assert.False(ConfigSectionValidator.IsValid(CopyForTest("single_removable_section_case.config")));
        }

    }
}
