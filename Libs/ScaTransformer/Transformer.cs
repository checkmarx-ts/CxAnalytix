using CxAnalytix.Interfaces.Transform;
using System;
using System.Threading;

namespace ScaTransformer
{
	public class Transformer : ITransformer
	{

		public Transformer()
		{
			// TODO: Instance-specific config goes here....

		}


		public void DoTransform(int concurrentThreads, string previousStatePath, string instanceId, IProjectFilter filter, CancellationToken token)
		{
			throw new NotImplementedException();
		}
	}
}
