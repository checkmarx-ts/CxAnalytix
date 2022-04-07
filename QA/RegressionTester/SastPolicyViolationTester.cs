using System;
using System.Collections.Generic;
using System.Text;

namespace RegressionTester
{
	class SastPolicyViolationTester : DictionaryRegressionTester
	{
		public SastPolicyViolationTester (String oldPath, String newPath) : base (oldPath, newPath)
		{

		}

		protected override string FileMask => "sast_policy_violations*";

		protected override string[] UniqueIdentifierKeys => new string[] { "ScanId", "RuleId", "ViolationId" };

		protected override string[] FilteredKeys => new string[] { };

		protected override string TestName => "SAST Policy Violations";
	}
}
