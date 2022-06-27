using System;
using System.Threading;

namespace CxAnalytix.Interfaces.Transform
{
	public interface ITransformer
	{
		void DoTransform(CancellationToken token);

	}
}
