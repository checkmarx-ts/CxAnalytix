using CxAnalytix.Extensions;
using CxRestClient.CXONE.Common;
using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.CXONE
{
    public static class CxScanSummary
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxScanSummary));

        private static String URL_SUFFIX = "api/scan-summary";

        private static readonly int MAX_FETCH_SCANS = 20;



        [JsonObject(MemberSerialization.OptIn)]
        public class Query
        {
            [JsonProperty(PropertyName = "queryID")]
            public String QueryId { get; internal set; }

            [JsonProperty(PropertyName = "queryName")]
            public String QueryName { get; internal set; }

            [JsonProperty(PropertyName = "severity")]
            public String QuerySeverity { get; internal set; }


            [JsonObject(MemberSerialization.OptIn)]
            public class StatusCounter
            {
                [JsonProperty(PropertyName = "status")]
                public String Status { get; internal set; }

                [JsonProperty(PropertyName = "counter")]
                public UInt32 Total { get; internal set; }

            }

            [JsonProperty(PropertyName = "statusCounters")]
            public List<StatusCounter> StatusCounts { get; internal set; }

            [JsonProperty(PropertyName = "counter")]
            public UInt32 QueryTotal { get; internal set; }

        }


        [JsonObject(MemberSerialization.OptIn)]
        public class SastCounters
        {
            [JsonProperty(PropertyName = "queriesCounters")]
            public QueryIndex Queries { get; internal set; }

        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ScanSummary
        {

            [JsonProperty(PropertyName = "tenantId")]
            public String TenantId { get; internal set; }

            [JsonProperty(PropertyName = "scanId")]
            public String ScanId { get; internal set; }

            [JsonProperty(PropertyName = "sastCounters")]
            public SastCounters SastCounters { get; internal set; }
        }



        public class ScanSummaryIndex : SingleIndexedCollection<ScanSummary, String>
        {
            public override string GetIndexKey(ScanSummary item) => item.ScanId;
        }

        public class QueryIndex : SingleIndexedCollection<Query, String>
        {
            public override string GetIndexKey(Query item) => item.QueryId;
        }



        [JsonObject(MemberSerialization.OptIn)]
        internal class Response : TotaledArray
        {
            [JsonProperty(PropertyName = "scansSummaries")]
            public ScanSummaryIndex Index { get; internal set; }
        }

        private static async Task<Response> GetScanSummaries(CxOneRestContext ctx, CancellationToken token, 
            IDictionary<String, String> parameters)
        {
            return await WebOperation.ExecuteGetAsync<Response>(ctx.Json.CreateClient, 
                JsonUtils.DeserializeResponse<Response>,
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX, parameters), ctx, token);
        }

        public static async Task<ScanSummaryIndex> GetScanSummaries(CxOneRestContext ctx, CancellationToken token, 
            ParallelOptions threadOpts, ICollection<String> scanIds)
        {
            Dictionary<String, String> commonParams = new();

            ScanSummaryIndex response = new();

            var task = Parallel.ForEachAsync(scanIds.AsCsvStringBatches(MAX_FETCH_SCANS), threadOpts, async (batch, ctoken) =>
            {
                var parameters = new Dictionary<String, String>(commonParams);
                parameters.Add("scan-ids", batch);

                response.SyncCombine((await GetScanSummaries(ctx, token, parameters)).Index);
            });

            await Task.WhenAll(task);

            return response;
        }

    }
}
