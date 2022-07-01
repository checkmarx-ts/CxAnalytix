using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.SCA
{
    public class CxScans
    {

        private static ILog _log = LogManager.GetLogger(typeof(CxScans));

        private static String URL_SUFFIX = "risk-management/scans";

        private CxScans()
        { }

        [JsonObject(MemberSerialization.OptIn)]
        public class Scan
        {
            [JsonProperty(PropertyName = "projectId")]
            public String ProjectId { get; internal set; }

            [JsonProperty(PropertyName = "createdOn")]
            internal String _created { get; set; }
            public DateTime Created => JsonUtils.NormalizeDateParse(_created);
            [JsonProperty(PropertyName = "lastUpdate")]
            internal String _updated { get; set; }
            public DateTime Updated => JsonUtils.NormalizeDateParse(_updated);

            [JsonProperty(PropertyName = "status")]
            public ScanStatus ScanStatus { get; internal set; }

            [JsonProperty(PropertyName = "origin")]
            public String Origin { get; internal set; }

            [JsonProperty(PropertyName = "riskReportId")]
            public String RiskReportId { get; internal set; }

            [JsonProperty(PropertyName = "scanId")]
            public String ScanId { get; internal set; }

            [JsonProperty(PropertyName = "revision")]
            public String Revision { get; internal set; }

            [JsonProperty(PropertyName = "username")]
            public String Username{ get; internal set; }

            [JsonProperty(PropertyName = "tenantId")]
            public String TenantId { get; internal set; }

            [JsonProperty(PropertyName = "scanProgress")]
            public List<ScanProgress> ScanProgress { get; internal set; }

            [JsonProperty(PropertyName = "tags")]
            public Dictionary<String, String> Tags { get; internal set; }

            [JsonProperty(PropertyName = "manifestFilesUploaded")]
            public Boolean ManifestFilesUploaded { get; internal set; }
            public override string ToString() =>
                $"{ProjectId}:{ScanId} Status: {ScanStatus.Status} Origin: {Origin}";

        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ScanProgress
        {
            [JsonProperty(PropertyName = "name")]
            public String Phase { get; internal set; }

            [JsonProperty(PropertyName = "startTime")]
            internal String _start { get; set; }
            public DateTime Start => JsonUtils.NormalizeDateParse(_start);

            [JsonProperty(PropertyName = "endTime")]
            internal String _end { get; set; }
            public DateTime End => JsonUtils.NormalizeDateParse(_end);

            [JsonProperty(PropertyName = "status")]
            public String Status { get; internal set; }
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class ScanStatus
        {
            [JsonProperty(PropertyName = "name")]
            public String Status { get; internal set; }
            [JsonProperty(PropertyName = "message")]
            public String Message { get; internal set; }
            [JsonProperty(PropertyName = "detailedStatus")]
            public Dictionary<String, ScanDetailedStatus> DetailedStatus { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ScanDetailedStatus
        {
            [JsonProperty(PropertyName = "status")]
            public String Status { get; internal set; }
            [JsonProperty(PropertyName = "errors")]
            public List<String> Errors { get; internal set; }
        }

        public static IEnumerable<Scan> GetScans(CxSCARestContext ctx, CancellationToken token, String projectId)
        {
            using (var r = WebOperation.ExecuteGet<JsonResponseArrayReader<Scan>>(ctx.Json.CreateClient,
                (response) => new JsonResponseArrayReader<Scan>(response.Content.ReadAsStreamAsync().Result),
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX,
                new Dictionary<String, String>() { { "ProjectId", projectId }}), ctx, token))
                return new List<Scan>(r);
        }

        public static IEnumerable<Scan> GetCompletedScans(CxSCARestContext ctx, CancellationToken token, String projectId)
        {
            return GetScans(ctx, token, projectId).Where((s) => s.ScanStatus.Status.CompareTo("Done") == 0);
        }


    }
}
