using System;
using System.Threading;

namespace CxAnalytix.Interfaces.Transform
{
	public interface ITransformer : IDisposable
	{
		void DoTransform(CancellationToken token);
		String DisplayName { get; }

	}
}
