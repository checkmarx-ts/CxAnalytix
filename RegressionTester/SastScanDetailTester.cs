using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RegressionTester
{
	public class SastScanDetailTester : DictionaryRegressionTester
	{
		private StreamWriter _out;

		private static readonly String FILE_BASE_NAME = "sast_scan_detail";

		public SastScanDetailTester(String oldPath, String newPath) : base (oldPath, newPath)
		{
			_out = new StreamWriter($"{FILE_BASE_NAME}-test.txt");

		}

		protected override string FileMask => $"{FILE_BASE_NAME}*";

		protected override string[] UniqueIdentifierKeys => new string[] { "ProjectId", "ScanId", "VulnerabilityId", "NodeId"};
		protected override string[] FilteredKeys => null;

		protected override string TestName => "SAST Scan Detail";

		protected override TextWriter OutputWriter => _out;

		public override void Dispose()
		{
			if (_out != null)
			{
				_out.Flush();
				_out.Close();
			}
		}
	}
}
