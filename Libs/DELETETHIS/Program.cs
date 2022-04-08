using ScaTransformer;
using System;
using System.Threading;

namespace DELETETHIS
{
	class Program
	{
		static void Main(string[] args)
		{

			var trf = new Transformer();

			using (CancellationTokenSource t = new CancellationTokenSource())
				trf.DoTransform(2, "C:\temp", null, null, t.Token);

			Console.WriteLine("Hello World!");
		}
	}
}
