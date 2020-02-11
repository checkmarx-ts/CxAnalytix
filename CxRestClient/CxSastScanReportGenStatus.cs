using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace CxRestClient
{
    public class CxSastScanReportGenStatus
    {
        private static String URL_SUFFIX = "cxrestapi/reports/sastScan/{0}/status";

        public enum GenStatus
        {
            None,
            InProcess,
            Created
        }


        private static GenStatus ReadStatus(JToken responsePayload)
        {
            var reader = new JTokenReader(responsePayload);

            if (JsonUtils.MoveToNextProperty(reader, "value"))
            {
                return Enum.Parse<GenStatus>(((JProperty)reader.CurrentToken).Value.ToString());
            }
            else
                throw new InvalidDataException("reportId missing in reponse payload");
        }


        public static GenStatus GetReportGenerationStatus (CxRestContext ctx, String reportId)
        {
            var client = ctx.Json.CreateClient();

            var scanReportStatus = client.GetAsync(
                CxRestContext.MakeUrl(ctx.Url, 
                String.Format (URL_SUFFIX, reportId) )).Result;

            if (!scanReportStatus.IsSuccessStatusCode)
                return GenStatus.None;

            JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                (scanReportStatus.Content.ReadAsStreamAsync().Result)));

            return ReadStatus(jt);
        }
    }
}
