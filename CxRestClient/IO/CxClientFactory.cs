using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient.Utility
{
    public class CxClientFactory
    {

        private static ILog _log = LogManager.GetLogger(typeof(CxClientFactory));

        private CxClientFactory()
        { }

        internal CxClientFactory(String mediaType, CxRestContext ctx)
        {
            Context = ctx;
            MediaType = mediaType;
        }

        private CxRestContext Context { get; set; }
        private String MediaType { get; set; }

        public CxRestClient CreateSastClient()
        {
            return new CxRestClient(new System.Net.Http.Headers.AuthenticationHeaderValue(Context.SastToken.TokenType,
                Context.SastToken.Token), MediaType);
        }

        public CxRestClient CreateMnoClient()
        {
            return new CxRestClient(new System.Net.Http.Headers.AuthenticationHeaderValue(Context.MNOToken.TokenType,
                Context.MNOToken.Token), MediaType);
        }
    }

}
