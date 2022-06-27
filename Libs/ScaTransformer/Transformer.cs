using CxAnalytix.Interfaces.Transform;
using SDK.Modules.Transformer;
using System;
using System.Threading;

namespace ScaTransformer
{
	public class Transformer : TransformerModule
	{
		private static readonly String STATE_STORAGE_FILE = "CxAnalytixExportState_SCA.json";

		public Transformer() : base("SAST-SCA", typeof(Transformer), STATE_STORAGE_FILE)
		{
		}

		public override void DoTransform(CancellationToken token)
		{
			throw new NotImplementedException();
		}
    }
}
