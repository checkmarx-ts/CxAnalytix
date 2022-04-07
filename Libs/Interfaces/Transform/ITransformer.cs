using System;
using System.Threading;

namespace CxAnalytix.Interfaces.Transform
{
	public interface ITransformer
	{

		// TODO: Missing transformer-specific settings:
		// RecordNames
		// CxSASTRestContext
		// includeMnO
		// includeOSA
		// May be able to use annotations to locate a factory class that properly creates the instance.
		void DoTransform(int concurrentThreads, String previousStatePath, String instanceId, IProjectFilter filter, CancellationToken token);

	}
}
