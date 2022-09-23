using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CxRestClient.CXONE
{
    public class CxProjects
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxProjects));

        private static String URL_SUFFIX = "api/projects";

        private CxProjects()
        { }


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
        public class ProjectCollection
        {
            [JsonProperty(PropertyName = "totalCount")]
            public UInt32 Total { get; internal set; }

            [JsonProperty(PropertyName = "filteredTotalCount")]
            public UInt32 FilteredTotal { get; internal set; }

            [JsonProperty(PropertyName = "projects")]
            public List<Project> Projects { get; internal set; }


        }

        public static ProjectCollection GetProjects(CxOneRestContext ctx, CancellationToken token)
        {
            return WebOperation.ExecuteGet<ProjectCollection>(ctx.Json.CreateClient,
                JsonUtils.DeserializeResponse<ProjectCollection>,
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX), ctx, token);
        }

    }
}
