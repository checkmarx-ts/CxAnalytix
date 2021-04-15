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
			Parser.Default.ParseArguments<Options>(args)
				.WithParsed<Options>(o =>
			   {
				   using (var sastDetailTask = new SastScanDetailTester(o.PreviousVersionLogPath, o.NewVersionLogPath).PerformTest())
				   {
					   sastDetailTask.Wait();
				   }
			   });
		}
	}
}
