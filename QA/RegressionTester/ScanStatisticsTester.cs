using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegressionTester
{
    class ScanStatisticsTester : DictionaryRegressionTester
    {
        public ScanStatisticsTester(String oldPath, String newPath) : base(oldPath, newPath)
        {

        }

        protected override string FileMask => "sast_scan_statistics*";

        protected override string[] UniqueIdentifierKeys => new string[] { "ScanId" };

        protected override string[] FilteredKeys => throw new NotImplementedException();

        protected override string TestName => "SAST Scan Statistics";
    }
}
