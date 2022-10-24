using System;
using System.Threading;
using CommandLine;
using CxAnalytix.Executive;


namespace CxAnalytixCLI
{
    class Program
    {
        public class CommandLineOpts
        {
            [Option('l', "loop", Default = false, Required = false, HelpText = "Do not exit, continue running in a loop like the service or daemon.")]
            public bool Loop { get; set; }
        }

        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<CommandLineOpts>(args).WithParsed((opts) =>
            {
                if (!opts.Loop)
                    ExecuteOnce.Execute();
                else
                {
                    var ctoken = new CancellationTokenSource();
                    Console.CancelKeyPress += (sender, args) =>
                    {
                        Console.WriteLine("CTRL-C: Exiting!");
                        ctoken.Cancel();
                    };

                    ExecuteLoop.Execute(ctoken);
                }
            });
        }

    }
}
