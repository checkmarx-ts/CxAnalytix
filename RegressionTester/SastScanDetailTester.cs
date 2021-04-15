using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RegressionTester
{
	class SastScanDetailTester : DictionaryRegressionTester
	{

		public SastScanDetailTester(String oldPath, String newPath) : base (oldPath, newPath)
		{
		}

		protected override string FileMask => "sast_scan_detail*";

		protected override string[] UniqueIdentifierKeys => new string[] { "ProjectId", "ScanId", "VulnerabilityId", "NodeId"};
		protected override string[] FilteredKeys => new string[] { };

		protected override string TestName => "SAST Scan Detail";

	}
}
