﻿using CxAnalytix.Utilities;
using CxRestClient.Utility;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.SAST
{
    public class CxSastDownloadReport
    {

        private static ILog _log = LogManager.GetLogger(typeof(CxSastDownloadReport));
        private static String URL_SUFFIX = "cxrestapi/reports/sastScan/{0}";
        private static int BUFFER_SIZE = 64738;

        private CxSastDownloadReport()
        { }

        public static Stream GetVulnerabilities(CxSASTRestContext ctx,
            CancellationToken token, String reportId)
        {
			return WebOperation.ExecuteGet<Stream>(
			ctx.Sast.Xml.CreateClient
			, (response) =>
            {
                var report = response.Content.ReadAsStreamAsync().Result;
                var mem = new SharedMemoryStream(response.Content.Headers.ContentLength.Value);

                int readAmount = 0;
                byte[] buffer = new byte[BUFFER_SIZE];

                do
                {
                    readAmount = report.Read(buffer, 0, BUFFER_SIZE);

                    mem.Write(buffer, 0, readAmount);

                    if (readAmount < BUFFER_SIZE)
                        mem.Seek(0, SeekOrigin.Begin);

                } while (readAmount == BUFFER_SIZE);

                return mem;
            }
			, UrlUtils.MakeUrl(ctx.Sast.ApiUrl, String.Format(URL_SUFFIX, reportId))
			, ctx.Sast
			, token);
        }
    }
}
