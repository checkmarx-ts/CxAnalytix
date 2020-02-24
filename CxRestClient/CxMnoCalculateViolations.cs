using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient
{
    public class CxMnoCalculateViolations
    {
        private static readonly String URL_SUFFIX = "cxarm/policymanager/projects" +
            "/{0}/violationscalculation";

        public static bool CalculateViolations (CxRestContext ctx,
                CancellationToken token, int projectId)
        {
            var client = ctx.Json.CreateMnoClient();

            var calculationResponse = client.PostAsync(CxRestContext.MakeUrl(ctx.MnoUrl,
                String.Format(URL_SUFFIX, projectId)), null, token).Result;

            return calculationResponse.StatusCode == HttpStatusCode.Created;
        }


    }
}
