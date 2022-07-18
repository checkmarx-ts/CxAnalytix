using System;
using System.Collections.Generic;
using System.Text;

namespace RegressionTester
{
	class ScaScanSummaryTester : DictionaryRegressionTester
	{
		public ScaScanSummaryTester (String oldPath, String newPath) : base(oldPath, newPath)
		{

		}

		protected override string FileMask => "sca_scan_summary*";

		protected override string[] UniqueIdentifierKeys => new string[] { "ScanId" };

		protected override string[] FilteredKeys => new string[] { } ;

		protected override string TestName => "SCA Scan Summary";
	}
}
