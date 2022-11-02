using System;
using System.Collections.Generic;
using System.Text;

namespace RegressionTester
{
	class SastProjectInfoTester : DictionaryRegressionTester
	{

		public SastProjectInfoTester(String oldPath, String newPath) : base(oldPath, newPath)
		{

		}

		protected override string FileMask => "sast_project_info*";

		protected override string[] UniqueIdentifierKeys => new string[] { "ProjectId" };

		protected override string[] FilteredKeys => new string[] { "LastCrawlDate" };

		protected override string TestName => "SAST Project Info";
	}
}
