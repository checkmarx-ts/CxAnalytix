using CxAnalytix.Interfaces.Transform;
using SDK.Modules.Transformer;
using System;
using System.Threading;

namespace CxAnalytix.XForm.ScaTransformer
{
	public class Transformer : TransformerModule
	{
		private static readonly String STATE_STORAGE_FILE = "CxAnalytixExportState_SCA.json";
		private static readonly String MODULE_NAME = "SCA";

		public Transformer() : base(MODULE_NAME, typeof(Transformer), STATE_STORAGE_FILE)
		{
		}

		public override string DisplayName => MODULE_NAME;


		public override void DoTransform(CancellationToken token)
		{
			throw new NotImplementedException();
		}
    }
}
