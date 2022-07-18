using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RegressionTester
{
	class SastScanSummaryTester : DictionaryRegressionTester
	{

		public SastScanSummaryTester(String oldPath, String newPath) : base(oldPath, newPath)
		{
		}

		protected override string FileMask => "sast_scan_summary*";

		protected override string[] UniqueIdentifierKeys => new string[] { "ScanId" };

		protected override string[] FilteredKeys => new string[] { "ReportCreationTime" };

		protected override string TestName => "SAST Scan Summary";

	}
}
