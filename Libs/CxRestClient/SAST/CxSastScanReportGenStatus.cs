﻿using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using CxRestClient.Utility;

namespace CxRestClient.SAST
{
    public class CxSastScanReportGenStatus
    {
        private static ILog _log = LogManager.GetLogger(typeof (CxSastScanReportGenStatus));

        private static String URL_SUFFIX = "cxrestapi/reports/sastScan/{0}/status";

        public enum GenStatus
        {
            None,
            InProcess,
            Created,
            Failed
        }


        private static GenStatus ReadStatus(JToken responsePayload)
        {
            using (var reader = new JTokenReader(responsePayload))
                if (JsonUtils.MoveToNextProperty(reader, "value"))
                {
                    return Enum.Parse<GenStatus>(((JProperty)reader.CurrentToken).Value.ToString());
                }
                else
                    throw new InvalidDataException("reportId missing in reponse payload");
        }


        public static GenStatus GetReportGenerationStatus(CxSASTRestContext ctx,
            CancellationToken token, String reportId)
        {
			return WebOperation.ExecuteGet<GenStatus>(
			ctx.Sast.Json.CreateClient
			, (response) =>
			{
				using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
				using (var jtr = new JsonTextReader(sr))
				{
					JToken jt = JToken.Load(jtr);
					return ReadStatus(jt);
				}
			}
			, UrlUtils.MakeUrl(ctx.Sast.ApiUrl, String.Format(URL_SUFFIX, reportId))
			, ctx.Sast
			, token);
		}
	}
}
