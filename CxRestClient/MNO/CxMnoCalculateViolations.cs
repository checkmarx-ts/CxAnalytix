using log4net;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace CxRestClient.MNO
{
    public class CxMnoCalculateViolations
    {
        public static ILog _log = LogManager.GetLogger(typeof (CxMnoCalculateViolations) );

        private static readonly String URL_SUFFIX = "cxarm/policymanager/projects" +
            "/{0}/violationscalculation";

        public static bool CalculateViolations(CxRestContext ctx,
                CancellationToken token, int projectId)
        {
            try
            {
                using (var client = ctx.Json.CreateMnoClient())
                using (var calculationResponse = client.PostAsync(CxRestContext.MakeUrl(ctx.MnoUrl,
                    String.Format(URL_SUFFIX, projectId)), null, token).Result)
                    return calculationResponse.StatusCode == HttpStatusCode.Created;
            }
            catch (HttpRequestException hex)
            {
                _log.Error("Communication error.", hex);
                throw hex;
            }
        }


    }
}
