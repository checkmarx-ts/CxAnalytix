using CxAnalytix.Exceptions;
using CxAnalytix.Extensions;
using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CxRestClient.SAST.CxPresets;
using static CxRestClient.SAST.CxSastScans;
using static CxRestClient.SAST.CxScanStatistics;
using static CxRestClient.SAST.CxVersion;

namespace CxRestClient.SAST
{
    public class CxScanStatistics
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxPresets));

        private static String ENDPOINT_PREFIX = "cxrestapi/sast/scans";

        private static String API_VERSION = "3.0";


        [JsonObject(MemberSerialization.OptIn)]
        public class GeneralQueries
        {
            [JsonProperty(PropertyName = "succeededGeneralQueriesCount")]
            public UInt32 SuccessCount { get; internal set; }

            [JsonProperty(PropertyName = "failedGeneralQueriesCount")]
            public UInt32 FailCount { get; internal set; }
        };

        [JsonObject(MemberSerialization.OptIn)]
        public class LanguageStatistics
        {
            [JsonObject(MemberSerialization.OptIn)]
            public class FileParsingStatistics
            {
                [JsonProperty(PropertyName = "parsedSuccessfullyCount")]
                public UInt32? SuccessCount { get; internal set; }

                [JsonProperty(PropertyName = "parsedUnsuccessfullyCount")]
                public UInt32? FailCount { get; internal set; }

                [JsonProperty(PropertyName = "parsedPartiallyCount")]
                public UInt32? PartialCount { get; internal set; }

            };

            [JsonObject(MemberSerialization.OptIn)]
            public class ScannedLOCStatistics
            {
                [JsonProperty(PropertyName = "successfulLOC")]
                public UInt32? SuccessCount { get; internal set; }

                [JsonProperty(PropertyName = "unsuccessfulLOC")]
                public UInt32? FailCount { get; internal set; }

                [JsonProperty(PropertyName = "scannedSuccessfullyLOCPercentage")]
                public UInt32? SuccessPercentage { get; internal set; }
            };

            [JsonProperty(PropertyName = "parsedFiles")]
            public FileParsingStatistics FileParseStats { get; internal set; }

            [JsonProperty(PropertyName = "scannedLOCPerLanguage")]
            public ScannedLOCStatistics LOCParseStats { get; internal set; }

            [JsonProperty(PropertyName = "countOfDomObjects")]
            public UInt32? DomObjectCount { get; internal set; }
        };


        [JsonObject(MemberSerialization.OptIn)]
        public class ScanStatistics
        {
            [JsonProperty(PropertyName = "id")]
            public String ScanId { get; internal set; }

            [JsonProperty(PropertyName = "scanId")]
            public String ScanGuid { get; internal set; }

            [JsonProperty(PropertyName = "scanStatus")]
            public ScanStatus ScanStatus { get; internal set; }

            [JsonProperty(PropertyName = "productVersion")]
            public String ProductVersion { get; internal set; }

            [JsonProperty(PropertyName = "engineVersion")]
            public String EngineVersion { get; internal set; }

            [JsonProperty(PropertyName = "memoryPeakInMB")]
            public String PeakPhysicalMemoryMB { get; internal set; }

            [JsonProperty(PropertyName = "virtualMemoryPeakInMB")]
            public String PeakVirtualMemoryMB { get; internal set; }

            [JsonProperty(PropertyName = "isIncrementalScan")]
            public bool IncrementalScan{ get; internal set; }

            [JsonProperty(PropertyName = "resultsCount")]
            public UInt32 ResultCount { get; internal set; }

            [JsonProperty(PropertyName = "totalUnScannedFilesCount")]
            public UInt32 UnscannedFileCount { get; internal set; }

            [JsonProperty(PropertyName = "fileCountOfDetectedButNotScannedLanguages")]
            public SortedDictionary<String, UInt32?> UnscannedFileCountByLanguage { get; internal set; }

            [JsonProperty(PropertyName = "totalFilteredParsedLOC")]
            public UInt32 FilteredParsedLOC { get; internal set; }

            [JsonProperty(PropertyName = "totalUnFilteredParsedLOC")]
            public UInt32 UnfilteredParsedLOC { get; internal set; }

            [JsonProperty(PropertyName = "languageStatistics")]
            public SortedDictionary<String, LanguageStatistics> LanguageStats { get; internal set; }

            [JsonProperty(PropertyName = "exclusionFoldersPattern")]
            public String ExclusionFoldersPattern { get; internal set; }

            [JsonProperty(PropertyName = "exclusionFilesPattern")]
            public String ExclusionFilesPattern { get; internal set; }

            [JsonProperty(PropertyName = "failedQueriesCount")]
            public UInt32 FailedQueriesCount { get; internal set; }

            [JsonProperty(PropertyName = "generalQueries")]
            public GeneralQueries GeneralQueryStats { get; internal set; }

            [JsonProperty(PropertyName = "failedStages")]
            public String FailedStages { get; internal set; }

            [JsonProperty(PropertyName = "engineOperatingSystem")]
            public String EngineOS { get; internal set; }

            [JsonProperty(PropertyName = "enginePackVersion")]
            public String EnginePackVersion { get; internal set; }

        };


        private CxScanStatistics()
        { }


        private static async Task<T> GetForSingleObjectResponse<T>(CxSASTRestContext ctx, CancellationToken token, String endpoint)
        {
            return await WebOperation.ExecuteGetAsync<T>(
                ctx.Sast.Json.CreateClient
                , JsonUtils.DeserializeResponse<T>
                , endpoint
                , ctx.Sast
                , token
                ,apiVersion: API_VERSION);
        }

        private static String STATISTICS_URL_SUFFIX = "statistics";
        public static async Task<ScanStatistics> GetScanStatistics(CxSASTRestContext ctx, CancellationToken token, String scanId)
        {
            var endpoint = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, ENDPOINT_PREFIX, scanId, STATISTICS_URL_SUFFIX);

            return await GetForSingleObjectResponse<ScanStatistics>(ctx, token, endpoint);
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class FileParsingDetails
        {
            [JsonProperty(PropertyName = "parsedSuccessfully")]
            public String SuccessfullyParsed { get; internal set; }

            [JsonProperty(PropertyName = "parsedUnsuccessfully")]
            public String FailedToParse { get; internal set; }

            [JsonProperty(PropertyName = "parsedPartially")]
            public String PartiallyParsed { get; internal set; }

        };

        [JsonObject(MemberSerialization.OptIn)]
        public abstract class StatisticsWrapper<T>
        {
            [JsonProperty(PropertyName = "id")]
            public String ScanId { get; internal set; }

            public abstract T Collection { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ParsedFilesResponse : StatisticsWrapper<SortedDictionary<string, FileParsingDetails> >
        {
            [JsonProperty(PropertyName = "scannedFilesPerLanguage")]
            public override SortedDictionary<string, FileParsingDetails> Collection { get ; internal set; }
        }


        private static String PARSEDFILES_URL_SUFFIX = "parsedFiles";

        public static async Task<ParsedFilesResponse> GetScanParsedFiles(CxSASTRestContext ctx, CancellationToken token, String scanId)
        {
            var endpoint = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, ENDPOINT_PREFIX, scanId, PARSEDFILES_URL_SUFFIX);

            return await GetForSingleObjectResponse<ParsedFilesResponse>(ctx, token, endpoint);
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class FailedQueriesResponse : StatisticsWrapper<List<String>>
        {
            [JsonProperty(PropertyName = "failedQueries")]
            public override List<String> Collection { get; internal set; }
        }

        private static String FAILEDQUERIES_URL_SUFFIX = "failedQueries";
        public static async Task<FailedQueriesResponse> GetScanFailedQueries(CxSASTRestContext ctx, CancellationToken token, String scanId)
        {
            var endpoint = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, ENDPOINT_PREFIX, scanId, FAILEDQUERIES_URL_SUFFIX);
            return await GetForSingleObjectResponse<FailedQueriesResponse>(ctx, token, endpoint);
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class FailedGeneralQueriesResponse : StatisticsWrapper<List<String>>
        {
            [JsonProperty(PropertyName = "failedGeneralQueries")]
            public override List<String> Collection { get; internal set; }
        }


        private static String FAILEDGENERALQUERIES_URL_SUFFIX = "failedGeneralQueries";
        public static async Task<FailedGeneralQueriesResponse> GetScanFailedGeneralQueries(CxSASTRestContext ctx, CancellationToken token, String scanId)
        {
            var endpoint = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, ENDPOINT_PREFIX, scanId, FAILEDGENERALQUERIES_URL_SUFFIX);
            return await GetForSingleObjectResponse<FailedGeneralQueriesResponse>(ctx, token, endpoint);
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class SuccessfulGeneralQueriesResponse : StatisticsWrapper<SortedDictionary<string, UInt32>>
        {
            [JsonProperty(PropertyName = "generalQueriesResultCount")]
            public override SortedDictionary<string, UInt32> Collection { get; internal set; }
        }

        private static String SUCCESSGENERALQUERIES_URL_SUFFIX = "succeededGeneralQueries";
        public static async Task<SuccessfulGeneralQueriesResponse> GetScanSuccessfulGeneralQueries(CxSASTRestContext ctx, CancellationToken token, String scanId)
        {
            var endpoint = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, ENDPOINT_PREFIX, scanId, SUCCESSGENERALQUERIES_URL_SUFFIX);
            return await GetForSingleObjectResponse<SuccessfulGeneralQueriesResponse>(ctx, token, endpoint);
        }

        public class FullScanStatistics
        {
            internal FullScanStatistics()
            { }

            public ScanStatistics Statistics { get; internal set; }
            public ParsedFilesResponse ParsedFiles { get; internal set; }
            public FailedQueriesResponse FailedQueries { get; internal set; }
            public FailedGeneralQueriesResponse FailedGeneralQueries { get; internal set; }
            public SuccessfulGeneralQueriesResponse SuccessGeneralQueries { get; internal set; }
        };

        public static FullScanStatistics GetScanFullStatistics(CxSASTRestContext ctx, CancellationToken token, String scanId)
        {
            CancellationTokenSource localToken = new();

            List<Task> runningTasks = new();

            using (token.Register(() => localToken.Cancel()))
                try
                {
                    var statistics = GetScanStatistics(ctx, localToken.Token, scanId);
                    runningTasks.Add(statistics);

                    var pf = GetScanParsedFiles(ctx, localToken.Token, scanId);
                    runningTasks.Add(pf);

                    var fq = GetScanFailedQueries(ctx, localToken.Token, scanId);
                    runningTasks.Add(fq);

                    var fgq = GetScanFailedGeneralQueries(ctx, localToken.Token, scanId);
                    runningTasks.Add(fgq);

                    var sgq = GetScanSuccessfulGeneralQueries(ctx, localToken.Token, scanId);
                    runningTasks.Add(sgq);


                    if (statistics.Result != null)
                        return new FullScanStatistics()
                        {
                            Statistics = statistics.Result,
                            ParsedFiles = pf.Result,
                            FailedQueries = fq.Result,
                            FailedGeneralQueries = fgq.Result,
                            SuccessGeneralQueries = sgq.Result
                        };
                }
                catch (Exception)
                {
                    localToken.Cancel();
                    throw;
                }
                finally
                {
                    runningTasks.SafeWaitAllToEnd();
                    runningTasks.DisposeTasks();
                }

            return null;
        }

    }
}
