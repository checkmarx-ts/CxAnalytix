using CxAnalytix.XForm.Common;
using log4net;

namespace CxAnalytix.XForm.CxOneTransformer
{
    public class Transformer : BaseTransformer
    {
        private static readonly ILog _log = LogManager.GetLogger(typeof(Transformer));
        private static readonly String STATE_STORAGE_FILE = "CxAnalytixExportState_CxOne.json";
        private static readonly String MODULE_NAME = "CxOne";

        public Transformer() : base(MODULE_NAME, typeof(Transformer), STATE_STORAGE_FILE)
        {
        }

        public override string DisplayName => "CheckmarxOne";

        public override void DoTransform(CancellationToken token)
        {
            throw new NotImplementedException();
        }
    }
}