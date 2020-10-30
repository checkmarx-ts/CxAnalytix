using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient
{
    public class CxSastDownloadReport
    {

        private static ILog _log = LogManager.GetLogger(typeof(CxSastDownloadReport));
        private static String URL_SUFFIX = "cxrestapi/reports/sastScan/{0}";
        private static readonly int RETRY_DELAY_MS = 30000;
        private static readonly int RETRY_MAX = 3;


        private CxSastDownloadReport()
        { }

        public static Stream GetVulnerabilities(CxRestContext ctx,
            CancellationToken token, String reportId)
        {
            try
            {
                int retryCount = 0;

                while (retryCount < RETRY_MAX)
                {
                    using (var client = ctx.Xml.CreateSastClient())
                    {
                        if (retryCount > 0)
                        {
                            int delay = RETRY_DELAY_MS * retryCount;
                            _log.Info($"Waiting {delay}ms before retrying download of report {reportId}");
                            Task.Delay(delay, token);
                        }

                        try
                        {
                            var reportPayload = client.GetAsync(CxRestContext.MakeUrl(ctx.Url,
                                String.Format(URL_SUFFIX, reportId)), token).Result;

                            if (!reportPayload.IsSuccessStatusCode)
                            {
                                _log.Warn($"Unable to retrieve XML report {reportId}. Response status [{reportPayload.StatusCode}:{reportPayload.ReasonPhrase}]." +
                                    $"  Attempt #{retryCount + 1} of {RETRY_MAX}.");

                                retryCount++;
                                continue;
                            }


                            return reportPayload.Content.ReadAsStreamAsync().Result;
                        }
                        catch (AggregateException aex)
                        {
                            _log.Warn($"Multiple exceptions caught attempting to retrieve XML report {reportId} during " +
                                $"attempt #{retryCount + 1} of {RETRY_MAX}.");

                            _log.Warn("BEGIN exception report");

                            int exCount = 0;

                            aex.Handle((x) =>
                          {
                              _log.Warn($"Exception #{++exCount}", x);

                              return true;
                          });

                            _log.Warn("END exception report");

                            retryCount++;
                        }
                        catch (Exception ex)
                        {
                            _log.Warn($"Exception caught attempting to retrieve XML report {reportId} during " +
                                $"attempt #{retryCount + 1} of {RETRY_MAX}.", ex);

                            retryCount++;
                        }
                    }
                }
            }
            catch (HttpRequestException hex)
            {
                _log.Error("Communication error.", hex);
                throw hex;
            }

            throw new InvalidOperationException($"Unable to retrieve XML report {reportId}.");
        }
    }
}
