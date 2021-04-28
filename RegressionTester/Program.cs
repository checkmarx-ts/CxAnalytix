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
			using (var policyViolations = new SastPolicyViolationTester(oldPath, newPath) ) 
			using (var scaDetail = new ScaScanDetailTester(oldPath, newPath))
			using (var scaSummary = new ScaScanSummaryTester(oldPath, newPath))
			using (var sastSummary = new SastScanSummaryTester(oldPath, newPath))
			using (var sastDetail = new SastScanDetailTester(oldPath, newPath))
			{
				var scaDetailTask = scaDetail.PerformTest();
				var scaSummaryTask = scaSummary.PerformTest();
				var sastDetailTask = sastDetail.PerformTest();
				var sastSummaryTask = sastSummary.PerformTest();
				var policyViolationsTask = policyViolations.PerformTest();
				var projectInfoTask = projectInfo.PerformTest();

				projectInfoTask.Wait();
				sastSummaryTask.Wait();
				sastDetailTask.Wait();
				scaSummaryTask.Wait();
				scaDetailTask.Wait();
				policyViolationsTask.Wait();
			}


		}
	}
}
