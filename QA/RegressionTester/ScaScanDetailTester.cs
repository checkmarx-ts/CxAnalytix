using System;
using System.Collections.Generic;
using System.Text;

namespace RegressionTester
{
	class ScaScanDetailTester : DictionaryRegressionTester
	{

		public ScaScanDetailTester (String oldPath, String newPath) : base(oldPath, newPath)
		{

		}


		protected override string FileMask => "sca_scan_detail*";

		protected override string[] UniqueIdentifierKeys => new string[] { "ScanId", "VulnerabilityId", "LibraryId" };

		protected override string[] FilteredKeys => new string[] { };

		protected override string TestName => "SCA Scan Detail";
	}
}
