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
    public static class CxScans
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxScans));

        private static String URL_SUFFIX = "api/scans";


        [JsonObject(MemberSerialization.OptIn)]
        public class Scan
        {
            [JsonProperty(PropertyName = "id")]
            public String ScanId { get; internal set; }

            [JsonProperty(PropertyName = "status")]
            public String Status { get; internal set; }

            [JsonProperty(PropertyName = "updatedAt")]
            public DateTime Updated{ get; internal set; }

            [JsonProperty(PropertyName = "projectId")]
            public String ProjectId { get; internal set; }

            [JsonProperty(PropertyName = "projectName")]
            public String ProjectName { get; internal set; }

            [JsonProperty(PropertyName = "sourceOrigin")]
            public String SourceOrigin { get; internal set; }

            [JsonProperty(PropertyName = "engines")]
            public List<String> EnginesForScan { get; set; }

            public String EnginesAsString => String.Join(";", EnginesForScan);

            public override string ToString()
            {
                return $"ScanId: {ScanId} Project: {ProjectName} ProjectId: {ProjectId}";
            }
        }



        [JsonObject(MemberSerialization.OptIn)]
        public class ScanCollection : WrappedArray
        {
            [JsonProperty(PropertyName = "scans")]
            public List<Scan> Scans { get; internal set; } = new();
        }


        public static async Task<ScanCollection> GetScans(CxOneRestContext ctx, CancellationToken token, String projectId, String[] statuses)
        {
            var parameters = new Dictionary<String, String> {
                { "project-id", projectId },
                { "statuses", String.Join("|", statuses) }
            };

            ScanCollection response = new ScanCollection();

            await PageableOperation.DoPagedGetRequest<ScanCollection>((scans) =>
            {
                response.Scans.AddRange(scans.Scans);

                return scans.Scans.Count;

            }, ctx.Json.CreateClient,
                JsonUtils.DeserializeResponse<ScanCollection>,
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX, parameters), ctx, token);

            return response;
        }

        public static async Task<ScanCollection> GetCompletedScans(CxOneRestContext ctx, CancellationToken token, String projectId)
        {
            return await GetScans(ctx, token, projectId, new string[] {"Completed"});
        }


    }
}
