using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.SCA
{
    public class CxProjects
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxProjects));

        private static String URL_SUFFIX = "risk-management/projects";

        private CxProjects()
        { }


        [JsonObject(MemberSerialization.OptIn)]

        public class Project
        {
            [JsonProperty(PropertyName = "id")]
            public String ProjectId { get; internal set; }
            [JsonProperty(PropertyName = "name")]
            public String ProjectName { get; internal set; }
            [JsonProperty(PropertyName = "isManaged")]
            public Boolean Managed { get; internal set; }
            [JsonProperty(PropertyName = "createdOn")]
            internal String _created { get; set; }
            public DateTime Created => JsonUtils.NormalizeDateParse(_created);

            [JsonProperty(PropertyName = "tenantId")]
            public String TenantId { get; internal set; }
            [JsonProperty(PropertyName = "branch")]
            public String Branch { get; internal set; }
            [JsonProperty(PropertyName = "assignedTeams")]
            public List<String> Teams{ get; internal set; }
            [JsonProperty(PropertyName = "lastSuccessfulScanId")]
            public String LastSuccessfulScanId { get; internal set; }
            [JsonProperty(PropertyName = "tags")]
            public Dictionary<String, String> Tags { get; internal set; }
            [JsonProperty(PropertyName = "latestScanId")]
            public String LatestScanId { get; internal set; }

            public override string ToString() =>
                $"{ProjectId}:{ProjectName} [Teams: [{String.Join(',', Teams)}] Managed: {Managed}]";

        }

        public class ProjectIndex : SingleIndexedCollection<Project, String>
        {
            public override string GetIndexKey(Project item) => item.ProjectId;
        }

        public static ProjectIndex GetProjects(CxSCARestContext ctx, CancellationToken token)
        {
            return WebOperation.ExecuteGet<ProjectIndex>(ctx.Json.CreateClient,
                JsonUtils.DeserializeResponse<ProjectIndex>,
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX), ctx, token);
        }

    }
}
