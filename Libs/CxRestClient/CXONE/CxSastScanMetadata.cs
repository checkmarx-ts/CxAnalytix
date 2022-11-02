using CxAnalytix.Extensions;
using CxRestClient.CXONE.Common;
using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.CXONE
{
    public static class CxSastScanMetadata
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxScans));

        private static String API_SUFFIX = "api/sast-metadata";
        private static String METRICS_SUFFIX = "metrics";


        [JsonObject(MemberSerialization.OptIn)]
        public class SastScanMetadata
        {
            [JsonProperty(PropertyName = "scanId")]
            public String ScanId { get; internal set; }

            [JsonProperty(PropertyName = "projectId")]
            public String ProjectId { get; internal set; }

            [JsonProperty(PropertyName = "loc")]
            public UInt64 LOC { get; internal set; }

            [JsonProperty(PropertyName = "fileCount")]
            public UInt32 FileCount { get; internal set; }

            [JsonProperty(PropertyName = "isIncremental")]
            public bool IsIncremental { get; internal set; }

            [JsonProperty(PropertyName = "queryPreset")]
            public String Preset { get; internal set; }
        }

        public class ScanMetadataIndex : SingleIndexedCollection<SastScanMetadata, String>
        {
            public override string GetIndexKey(SastScanMetadata item) => item.ScanId;
        }


        [JsonObject(MemberSerialization.OptIn)]
        internal class MetadataResponse : TotaledArray
        {
            [JsonProperty(PropertyName = "scans")]
            public ScanMetadataIndex Index { get; internal set; }
        }



        public static async Task<ScanMetadataIndex> GetScanMetadata(CxOneRestContext ctx, CancellationToken token,
            ParallelOptions threadOpts, ICollection<String> scanIds)
        {
            ScanMetadataIndex result = new ScanMetadataIndex();

            var task = Parallel.ForEachAsync(scanIds.AsCsvStringBatches(), threadOpts, async (batch, ctoken) =>
            {
                var parameters = new Dictionary<String, String>() {
                    { "scan-ids", batch}
                };

                result.SyncCombine((await WebOperation.ExecuteGetAsync<MetadataResponse>(ctx.Json.CreateClient, 
                    JsonUtils.DeserializeResponse<MetadataResponse>, 
                    UrlUtils.MakeUrl(ctx.ApiUrl, API_SUFFIX, parameters), 
                    ctx, 
                    token)).Index);
            });

            await Task.WhenAll(task);

            return result;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class FileCountMetrics
        {
            [JsonProperty(PropertyName = "goodFiles")]
            public UInt64 GoodFiles { get; internal set; }

            [JsonProperty(PropertyName = "partiallyGoodFiles")]
            public UInt64 PartiallyGoodFiles { get; internal set; }

            [JsonProperty(PropertyName = "badFiles")]
            public UInt64 BadFiles { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class SastScanMetrics
        {
            [JsonProperty(PropertyName = "scanId")]
            public String ScanId { get; internal set; }

            [JsonProperty(PropertyName = "memoryPeak")]
            public UInt64 MemoryPeak { get; internal set; }

            [JsonProperty(PropertyName = "virtualMemoryPeak")]
            public UInt64 VirtualMemoryPeak { get; internal set; }

            [JsonProperty(PropertyName = "totalScannedLoc")]
            public UInt64 LOC { get; internal set; }

            [JsonProperty(PropertyName = "domObjectsPerLanguage", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<String, UInt64> DomObjectsByLanguage = new();

            [JsonProperty(PropertyName = "successfullLocPerLanguage", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<String, UInt64> ParsedLOCByLanguage = new();

            [JsonProperty(PropertyName = "failedLocPerLanguage", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<String, UInt64> ParseFailureLOCByLanguage = new();

            [JsonProperty(PropertyName = "fileCountOfDetectedButNotScannedLanguages", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<String, UInt64> UnscannedFilesByLanguage = new();

            [JsonProperty(PropertyName = "scannedFilesPerLanguage", NullValueHandling = NullValueHandling.Ignore)]
            public Dictionary<String, FileCountMetrics> ScannedFilesByLanguage = new();

        }

        public static async Task<SastScanMetrics> GetScanMetrics(CxOneRestContext ctx, CancellationToken token, String scanId)
        {
            return await WebOperation.ExecuteGetAsync<SastScanMetrics>(
                ctx.Json.CreateClient, JsonUtils.DeserializeResponse<SastScanMetrics>, 
                UrlUtils.MakeUrl(ctx.ApiUrl, API_SUFFIX, scanId, METRICS_SUFFIX),
                ctx, token);
        }


    }
}
