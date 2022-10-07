using CxRestClient.CXONE.Common;
using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.CXONE
{
    public static class CxApplications
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxApplications));

        private static String URL_SUFFIX = "api/applications";

        [JsonObject(MemberSerialization.OptIn)]
        public class Application : IMultiKeyed<List<String>>
        {
            [JsonProperty(PropertyName = "id")]
            public String Id { get; internal set; }
            
            [JsonProperty(PropertyName = "name")]
            public String Name { get; internal set; }
            
            [JsonProperty(PropertyName = "criticality")]
            public UInt32 Criticality { get; internal set; }

            [JsonProperty(PropertyName = "tags")]
            public Dictionary<String, String> Tags { get; internal set; }

            [JsonProperty(PropertyName = "projectIds")]
            public List<String> ProjectIds { get; internal set; }

            public List<String> GetKeys() => ProjectIds;
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Response : FilteredTotaledArray
        {
            [JsonProperty(PropertyName = "applications")]
            public ApplicationIndex Applications { get; internal set; }
        }


        public class ApplicationIndex : MultiIndexCollection<Application, String>
        {
            public override List<String> GetIndexKeys(Application item) => item.ProjectIds;
        }


        public static async Task<ApplicationIndex> GetApplications(CxOneRestContext ctx, CancellationToken token)
        {
            return await WebOperation.ExecuteGetAsync<ApplicationIndex>(ctx.Json.CreateClient,
                (response) => JsonUtils.DeserializeResponse<Response>(response).Applications,
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX),
                ctx, token);
        }

    }
}
