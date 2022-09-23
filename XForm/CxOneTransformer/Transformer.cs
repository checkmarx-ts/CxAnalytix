using CxAnalytix.Configuration.Impls;
using CxAnalytix.XForm.Common;
using CxRestClient;
using log4net;
using CxOneConnection = CxAnalytix.XForm.CxOneTransformer.Config.CxOneConnection;

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
            ThreadOpts.CancellationToken = token;

            var conCfg = Configuration.Impls.Config.GetConfig<CxOneConnection>();
            var creds = Configuration.Impls.Config.GetConfig<CxApiTokenCredentials>();


            var restBuilder = new CxOneRestContext.CxOneRestContextBuilder();
            restBuilder.WithApiURL(conCfg.URL)
                .WithIAMUrl(conCfg.IamUrl)
                .WithOpTimeout(conCfg.TimeoutSeconds)
                .WithSSLValidate(conCfg.ValidateCertificates)
                .WithApiToken(creds.Token)
                .WithTenant(creds.Tenant)
                .WithRetryLoop(conCfg.RetryLoop);

            var ctx = restBuilder.Build();

            var foo = ctx.Token;
        }
    }
}