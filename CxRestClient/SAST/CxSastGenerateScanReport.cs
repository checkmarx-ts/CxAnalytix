using log4net;
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
using CxRestClient.Utility;

namespace CxRestClient.SAST
{
    public class CxSastGenerateScanReport
    {

        private static ILog _log = LogManager.GetLogger(typeof (CxSastGenerateScanReport));

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
            using (var reader = new JTokenReader(responsePayload))
                if (JsonUtils.MoveToNextProperty(reader, "reportId"))
                {
                    return ((JProperty)reader.CurrentToken).Value.ToString();
                }
                else
                    throw new InvalidDataException("reportId missing in reponse payload");

        }

        public static String GetGeneratedReportId(CxRestContext ctx, CancellationToken token, String scanId)
        {
            return GetGeneratedReportId(ctx, token, scanId, ReportTypes.XML);
        }


        public static String GetGeneratedReportId(CxRestContext ctx, CancellationToken token,
            String scanId, ReportTypes type)
        {
            var dict = new Dictionary<String, String>()
                {
                    { "reportType", type.ToString ()},
                    { "scanId", scanId }
                };

			using (var payload = new FormUrlEncodedContent(dict))
				return WebOperation.ExecutePost<String>(
				ctx.Json.CreateSastClient
				, (response) =>
				{
                    using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                    using (var jtr = new JsonTextReader(sr))
                    {
                        JToken jt = JToken.Load(jtr);
                        return ReadReportId(jt);
                    }
                }
                , CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX)
                , payload
                , ctx
				, token);
        }
    }
}
