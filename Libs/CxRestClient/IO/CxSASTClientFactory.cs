using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient.IO
{
    public class CxSASTClientFactory
    {

        private static ILog _log = LogManager.GetLogger(typeof(CxSASTClientFactory));

        private CxSASTClientFactory()
        { }

        internal CxSASTClientFactory(String mediaType, CxSASTRestContext ctx)
        {
            Context = ctx;
            MediaType = mediaType;
        }

        private CxSASTRestContext Context { get; set; }
        private String MediaType { get; set; }

        public CxRestClient CreateSastClient(String apiVersion)
        {
            return new CxRestClient(new System.Net.Http.Headers.AuthenticationHeaderValue(Context.SastToken.TokenType,
                Context.SastToken.Token), MediaType, apiVersion);
        }

        public CxRestClient CreateMnoClient(String apiVersion)
        {
            if (!Context.MNOToken.HasValue)
                return null;

            return new CxRestClient(new System.Net.Http.Headers.AuthenticationHeaderValue(Context.MNOToken.Value.TokenType,
                Context.MNOToken.Value.Token), MediaType, apiVersion);
        }
    }

}
