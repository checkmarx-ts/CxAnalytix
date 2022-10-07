using CxRestClient.CXONE.Common;
using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CxRestClient.CXONE.CxScanSummary;

namespace CxRestClient.CXONE
{
    public static class CxScans
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxScans));

        private static String URL_SUFFIX = "api/scans";


        [JsonObject(MemberSerialization.OptIn)]
        public class ScanMetadata
        {
            public bool SastIncrementalScan { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Scan
        {
            [JsonProperty(PropertyName = "id")]
            public String ScanId { get; internal set; }

            [JsonProperty(PropertyName = "status")]
            public String Status { get; internal set; }

            [JsonProperty(PropertyName = "branch")]
            public String Branch { get; internal set; }

            [JsonProperty(PropertyName = "createdAt")]
            public DateTime Created { get; internal set; }

            [JsonProperty(PropertyName = "updatedAt")]
            public DateTime Updated { get; internal set; }

            [JsonProperty(PropertyName = "projectId")]
            public String ProjectId { get; internal set; }

            [JsonProperty(PropertyName = "projectName")]
            public String ProjectName { get; internal set; }

            [JsonProperty(PropertyName = "initiator")]
            public String Initiator { get; internal set; }

            [JsonProperty(PropertyName = "tags")]
            public Dictionary<String, String> Tags { get; internal set; }

            [JsonProperty(PropertyName = "sourceOrigin")]
            public String SourceOrigin { get; internal set; }

            [JsonProperty(PropertyName = "sourceType")]
            public String SourceType { get; internal set; }

            [JsonProperty(PropertyName = "engines")]
            public List<String> EnginesForScan { get; set; }

            public String EnginesAsString => String.Join(";", EnginesForScan);

            [JsonProperty(PropertyName = "metadata")]
            ScanMetadata Metadata { get; set; }

            public String ScanType => (Metadata.SastIncrementalScan) ? "Incremental" : "Full";
            
            public override string ToString()
            {
                return $"ScanId: {ScanId} Project: {ProjectName} ProjectId: {ProjectId}";
            }
        }


        public class ScanIndex : SingleIndexedCollection<Scan, String>
        {
            public override string GetIndexKey(Scan item) => item.ScanId;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ScanCollection : FilteredTotaledArray
        {
            [JsonProperty(PropertyName = "scans")]
            public ScanIndex Scans { get; internal set; } = new();
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
                response.Scans.SyncCombine(scans.Scans);

                return scans.Scans.Count;
            }, ctx.Json.CreateClient,
                (response) => JsonUtils.DeserializeResponse<ScanCollection>(response, new ScanMetadataConverter() ),
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX, parameters), ctx, token);

            return response;
        }

        public static async Task<ScanCollection> GetCompletedScans(CxOneRestContext ctx, CancellationToken token, String projectId)
        {
            return await GetScans(ctx, token, projectId, new string[] {"Completed"});
        }


        private class ScanMetadataConverter : JsonConverter
        {
            public override bool CanConvert(Type objectType) => (objectType == typeof(ScanMetadata));

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                ScanMetadata returnValue = new();

                var token = JToken.Load(reader);

                foreach (var configEntry in token.Value<JArray>("configs"))
                {
                    var configType = configEntry.Value<String>("type");

                    if (!String.IsNullOrEmpty(configType) && configType.CompareTo("sast") == 0)
                    {
                        var valueObject = configEntry.Value<JToken>("value");

                        if (valueObject != null && valueObject.HasValues)
                            returnValue.SastIncrementalScan = valueObject.Value<bool>("incremental");

                        break;
                    }

                }

                return returnValue;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                throw new NotImplementedException();
            }
        }
    }
}
