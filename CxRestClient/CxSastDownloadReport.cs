using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CxRestClient
{
    public class CxSastDownloadReport
    {

        private static ILog _log = LogManager.GetLogger(typeof (CxSastDownloadReport) );
        private static String URL_SUFFIX = "cxrestapi/reports/sastScan/{0}";

        private CxSastDownloadReport ()
        { }

        public static Stream GetVulnerabilities(CxRestContext ctx, 
            CancellationToken token, String reportId)
        {
            var client = ctx.Xml.CreateSastClient();

            var reportPayload = client.GetAsync(CxRestContext.MakeUrl (ctx.Url, 
                String.Format (URL_SUFFIX, reportId) ) ).Result;

            if (!reportPayload.IsSuccessStatusCode)
                throw new InvalidOperationException($"Unable to retrieve report {reportId}.");

            return reportPayload.Content.ReadAsStreamAsync().Result;
        }


    }
}
