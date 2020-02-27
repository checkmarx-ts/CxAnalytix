using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient
{

    /// <summary>
    /// A class making 
    /// </summary>
    public class CxSastXmlReport
    {
        private static ILog _log = LogManager.GetLogger(typeof (CxSastXmlReport) );
        private static int DELAY_MS = 10000;

        public static Stream GetXmlReport (CxRestContext ctx, CancellationToken token, String scanId)
        {
            String reportId = CxSastGenerateScanReport.GetGeneratedReportId(ctx, token, scanId);

            CxSastScanReportGenStatus.GenStatus status = CxSastScanReportGenStatus.GenStatus.None;
            DateTime quitTime = DateTime.Now.Add(ctx.Timeout);
            do
            {
                Task.Delay(DELAY_MS, token);
                status = CxSastScanReportGenStatus.GetReportGenerationStatus(ctx, token, reportId);
                if (DateTime.Now.CompareTo(quitTime) > 0)
                {
                    _log.Warn($"Failed to retrive scan " +
                        $"report {reportId} for scan id {scanId}. " +
                        $"Vulnerability details will not be available.");
                    break;
                }
            } while (status != CxSastScanReportGenStatus.GenStatus.Created);

            if (status == CxSastScanReportGenStatus.GenStatus.Created)
            {
                _log.Debug($"Report Id {reportId} for scan id {scanId} created.");

                var report = CxSastDownloadReport.GetVulnerabilities(ctx, token, reportId);

                return report;
            }
            else
                throw new InvalidDataException("XML report stream is invalid.");
        }


    }
}
