using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient
{
    public class CxSastGenerateScanReport
    {

        private static String URL_SUFFIX = "cxrestapi/reports/sastScan";

        public enum ReportTypes
        {
            PDF,
            RTF,
            CSV,
            XML
        }

        private static String ReadReportId(JToken responsePayload)
        {
            var reader = new JTokenReader(responsePayload);

            if (JsonUtils.MoveToNextProperty(reader, "reportId"))
            {
                return ((JProperty)reader.CurrentToken).Value.ToString ();
            }
            else
                throw new InvalidDataException("reportId missing in reponse payload");

        }

        public static String GetGeneratedReportId (CxRestContext ctx, CancellationToken token, String scanId)
        {
            return GetGeneratedReportId(ctx, token, scanId, ReportTypes.XML);
        }


        public static String GetGeneratedReportId(CxRestContext ctx, CancellationToken token, 
            String scanId, ReportTypes type)
        {
            var client = ctx.Json.CreateClient();

            var dict = new Dictionary<String, String>()
            {
                { "reportType", type.ToString ()},
                { "scanId", scanId }
            };

            var payload = new FormUrlEncodedContent(dict);

            var scanReportTicket = client.PostAsync(
                CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX), payload).Result;

            if (!scanReportTicket.IsSuccessStatusCode)
                throw new InvalidOperationException
                    ($"Scan report generation request for scan {scanId} returned " +
                    $"{scanReportTicket.StatusCode}");

            JToken jt = JToken.Load(new JsonTextReader(new StreamReader
                (scanReportTicket.Content.ReadAsStreamAsync().Result)));

            return ReadReportId(jt);
        }

    }
}
