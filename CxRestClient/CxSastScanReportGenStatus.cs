using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;

namespace CxRestClient
{
    public class CxSastScanReportGenStatus
    {
        private static ILog _log = LogManager.GetLogger(typeof (CxSastScanReportGenStatus));

        private static String URL_SUFFIX = "cxrestapi/reports/sastScan/{0}/status";

        public enum GenStatus
        {
            None,
            InProcess,
            Created
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


        public static GenStatus GetReportGenerationStatus(CxRestContext ctx,
            CancellationToken token, String reportId)
        {
            try
            {
                var client = ctx.Json.CreateSastClient();
                
				var scanReportStatus = client.GetAsync(
					CxRestContext.MakeUrl(ctx.Url,
					String.Format(URL_SUFFIX, reportId)), token).Result;
				
				if (!scanReportStatus.IsSuccessStatusCode)
					return GenStatus.None;

				using (var sr = new StreamReader
						(scanReportStatus.Content.ReadAsStreamAsync().Result))
				using (var jtr = new JsonTextReader(sr))
				{
					JToken jt = JToken.Load(jtr);
					return ReadStatus(jt);
				}
            }
            catch (HttpRequestException hex)
            {
                _log.Error("Communication error.", hex);
                throw hex;
            }
        }
    }
}
