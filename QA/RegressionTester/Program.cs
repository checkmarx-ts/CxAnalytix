using CommandLine;
using System;

namespace RegressionTester
{
	class Program
	{

		public class Options
		{
			[Option('p', "previous", Required=true, HelpText = "Path to log files from previous version")]
			public String PreviousVersionLogPath { get; set; }
			[Option('n', "new", Required = true, HelpText = "Path to log files from new version")]
			public String NewVersionLogPath { get; set; }
		}


		static void Main(string[] args)
		{

			String oldPath = null;
			String newPath = null;

			Parser.Default.ParseArguments<Options>(args)
				.WithParsed<Options>(o =>
			   {
				   oldPath = o.PreviousVersionLogPath;
				   newPath = o.NewVersionLogPath;

			   });

			using (var projectInfo = new SastProjectInfoTester(oldPath, newPath))
			using (var policyViolations = new SastPolicyViolationTester(oldPath, newPath))
			using (var scaDetail = new ScaScanDetailTester(oldPath, newPath))
			using (var scaSummary = new ScaScanSummaryTester(oldPath, newPath))
			using (var sastSummary = new SastScanSummaryTester(oldPath, newPath))
			using (var sastDetail = new SastScanDetailTester(oldPath, newPath))
            using (var sastStats = new ScanStatisticsTester(oldPath, newPath))
            {
				using (var scaDetailTask = scaDetail.PerformTest())
				using (var scaSummaryTask = scaSummary.PerformTest())
				using (var sastDetailTask = sastDetail.PerformTest())
				using (var sastSummaryTask = sastSummary.PerformTest())
				using (var policyViolationsTask = policyViolations.PerformTest())
				using (var projectInfoTask = projectInfo.PerformTest())
				using (var statsTask = sastStats.PerformTest())
				{
					projectInfoTask.Wait();
					sastSummaryTask.Wait();
					sastDetailTask.Wait();
					scaSummaryTask.Wait();
					scaDetailTask.Wait();
					policyViolationsTask.Wait();
					statsTask.Wait();
				}
            }


		}
	}
}
