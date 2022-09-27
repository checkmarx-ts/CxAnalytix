using CxRestClient.CXONE.Common;
using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.CXONE
{
    public static class CxProjects
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxProjects));

        private static String URL_SUFFIX = "api/projects";


        [JsonObject(MemberSerialization.OptIn)]

        public class Project
        {
            [JsonProperty(PropertyName = "id")]
            public String ProjectId { get; internal set; }
            [JsonProperty(PropertyName = "name")]
            public String ProjectName { get; internal set; }
            [JsonProperty(PropertyName = "createdAt")]
            internal String _created { get; set; }
            public DateTime Created => JsonUtils.NormalizeDateParse(_created);

            [JsonProperty(PropertyName = "updatedAt")]
            internal String _updated { get; set; }
            public DateTime Updated => JsonUtils.NormalizeDateParse(_updated);

            [JsonProperty(PropertyName = "mainBranch")]
            public String MainBranch { get; internal set; }

            [JsonProperty(PropertyName = "groups")]
            public List<String> Groups { get; internal set; }

            [JsonProperty(PropertyName = "tags")]
            public Dictionary<String, String> Tags { get; internal set; }

            [JsonProperty(PropertyName = "repoUrl")]
            public String RepoUrl { get; internal set; }

            [JsonProperty(PropertyName = "criticality")]
            public UInt32 Criticality { get; internal set; }

            public override string ToString() =>
                $"{ProjectId}:{ProjectName} [Groups: [{String.Join(',', Groups)}] Criticality: {Criticality}]";
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ProjectCollection : WrappedArray
        {
            [JsonProperty(PropertyName = "projects")]
            public List<Project> Projects { get; internal set; }
        }

        public static async Task<List<Project>> GetProjects(CxOneRestContext ctx, CancellationToken token)
        {
            List<Project> response = new();

            await PageableOperation.DoPagedGetRequest<ProjectCollection>((pc) =>
            {

                if (pc != null && pc.Projects != null)
                {
                    response.AddRange(pc.Projects);
                    return pc.Projects.Count;
                }

                return 0;
            }, ctx.Json.CreateClient,
                JsonUtils.DeserializeResponse<ProjectCollection>,
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX), ctx, token);

            return response;
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class ScanDescriptor
        {
            [JsonProperty(PropertyName = "id")]
            public String ScanId { get; internal set; }

            [JsonProperty(PropertyName = "updatedAt")]
            public DateTime Completed { get; internal set; }

            [JsonProperty(PropertyName = "sourceOrigin")]
            public String Origin { get; internal set; }
        }


        public static async Task<Dictionary<String, ScanDescriptor>> GetProjectLatestScans(CxOneRestContext ctx, CancellationToken token, String scanStatus)
        {

            Dictionary<String, ScanDescriptor> response = new();

            Dictionary<String, String> parameters = new() { { "scan-status", scanStatus } };


            var url = UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX, "last-scan");

            await PageableOperation.DoPagedGetRequest<Dictionary<String, ScanDescriptor >> ((pc) =>
            {
                foreach (var key in pc.Keys)
                    response.Add(key, pc[key]);

                return pc.Keys.Count;

            }, ctx.Json.CreateClient,
                JsonUtils.DeserializeResponse<Dictionary<String, ScanDescriptor>>,
                UrlUtils.MakeUrl(url, parameters), ctx, token);


            return response;
        }


        public static async Task<Dictionary<String, ScanDescriptor>> GetProjectLatestCompletedScans(CxOneRestContext ctx, CancellationToken token)
        {
            return await GetProjectLatestScans(ctx, token, "Completed");
        }


    }
}
