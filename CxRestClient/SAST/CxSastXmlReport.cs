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

    /// <summary>
    /// A class aggregating the logic needed to retrieve an XML report from the
    /// CxSAST web api.
    /// </summary>
    public class CxSastXmlReport
    {
        private static ILog _log = LogManager.GetLogger(typeof (CxSastXmlReport) );
        private static int DELAY_MS = 10000;

        public static Stream GetXmlReport(CxRestContext ctx, CancellationToken token, String scanId)
        {
            try
            {
                String reportId = CxSastGenerateScanReport.GetGeneratedReportId(ctx, token, scanId);

                CxSastScanReportGenStatus.GenStatus status = CxSastScanReportGenStatus.GenStatus.None;
                DateTime quitTime = DateTime.Now.Add(ctx.Timeout);
                do
                {
                    Task.Delay(DELAY_MS, token).Wait ();
                    status = CxSastScanReportGenStatus.GetReportGenerationStatus(ctx, token, reportId);
                    if (DateTime.Now.CompareTo(quitTime) > 0)
                    {
                        _log.Warn($"Failed to retrive scan " +
                            $"report {reportId} for scan id {scanId}. The report failed to generate in less than {ctx.Timeout.TotalSeconds} seconds." +
                            $"Vulnerability details will not be available.");
                        break;
                    }
                } while (status != CxSastScanReportGenStatus.GenStatus.Created);

                if (status == CxSastScanReportGenStatus.GenStatus.Created)
                {
                    _log.Debug($"Report Id {reportId} for scan id {scanId} created.");

                    return CxSastDownloadReport.GetVulnerabilities(ctx, token, reportId);
                }
                else
                    throw new InvalidDataException($"XML report stream is invalid. Last generation status: {status}");
            }
            catch (HttpRequestException hex)
            {
                _log.Error("Communication error.", hex);
                throw hex;
            }
        }
    }
}
