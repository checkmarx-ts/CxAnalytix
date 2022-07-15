using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using SDK.Modules.Transformer.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.SCA
{
    public class CxPolicies
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxPolicies));

        private static String URL_SUFFIX = "policy-management/policies";


        [JsonObject(MemberSerialization.OptIn)]
        public class Policy
        {
            [JsonProperty(PropertyName = "id")]
            public String PolicyId { get; internal set; }
            [JsonProperty(PropertyName = "name")]
            public String Name { get; internal set; }
            [JsonProperty(PropertyName = "description")]
            public String Description { get; internal set; }
            [JsonProperty(PropertyName = "creationTime")]
            internal String _created { get; set; }
            public DateTime Created => JsonUtils.NormalizeDateParse(_created);
            [JsonProperty(PropertyName = "lastUpdateTime")]
            internal String _updated { get; set; }
            public DateTime Updated => JsonUtils.NormalizeDateParse(_updated);

            // TODO: RULES

            [JsonProperty(PropertyName = "projectIds")]
            public List<String> Projects { get; internal set; }

            [JsonProperty(PropertyName = "isDisabled")]
            public Boolean Disabled { get; internal set; }
            [JsonProperty(PropertyName = "isDefault")]
            public Boolean Global { get; internal set; }
            [JsonProperty(PropertyName = "isPredefined")]
            public Boolean Predefined { get; internal set; }

            [JsonProperty(PropertyName = "actions")]
            internal Dictionary<String, Object> _actions {get; set; }
            public Boolean BreakBuild => _actions.ContainsKey("breakBuild") && _actions["brealBuild"]  != null ? (Boolean)(_actions["brealBuild"]) : false;

        }

        public static IEnumerable<Policy> GetPolicies(CxSCARestContext ctx, CancellationToken token)
        {
            using (var r = WebOperation.ExecuteGet<JsonResponseArrayReader<Policy>>(ctx.Json.CreateClient,
                (response) => new JsonResponseArrayReader<Policy>(response.Content.ReadAsStream()),
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX), ctx, token))
                return new List<Policy>(r);
        }

    }
}
