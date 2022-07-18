using CxRestClient.Interfaces;
using log4net;
using System;
using System.Collections.Generic;
using System.Text;

namespace CxRestClient.IO
{
    public class CxRestClientFactory
    {

        private static ILog _log = LogManager.GetLogger(typeof(CxRestClientFactory));

        private CxRestClientFactory()
        { }

        internal CxRestClientFactory(String mediaType, ICxRestContext ctx)
        {
            Context = ctx;
            MediaType = mediaType;
        }

        private ICxRestContext Context { get; set; }
        private String MediaType { get; set; }

        public CxRestClient CreateClient(String apiVersion)
        {
            if (Context.Token == null)
                return null;

            return new CxRestClient(new System.Net.Http.Headers.AuthenticationHeaderValue(Context.Token.TokenType,
                Context.Token.Token), MediaType, apiVersion);
        }
    }

}
