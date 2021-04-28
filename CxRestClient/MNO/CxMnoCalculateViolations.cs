using CxRestClient.Utility;
using log4net;
using System;
using System.Net;
using System.Net.Http;
using System.Threading;

namespace CxRestClient.MNO
{
	public class CxMnoCalculateViolations
	{
		public static ILog _log = LogManager.GetLogger(typeof(CxMnoCalculateViolations));

		private static readonly String URL_SUFFIX = "cxarm/policymanager/projects" +
			"/{0}/violationscalculation";

		public static bool CalculateViolations(CxRestContext ctx,
				CancellationToken token, int projectId)
		{
			return WebOperation.ExecutePost<bool>(
			ctx.Json.CreateMnoClient
			, (response) => response.StatusCode == HttpStatusCode.Created
			, CxRestContext.MakeUrl(ctx.MnoUrl, String.Format(URL_SUFFIX, projectId))
			, null
			, ctx
			, token);
		}


	}
}
