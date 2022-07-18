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
    public class CxRiskState
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxRiskState));

        private static String URL_SUFFIX = "risk-management/risk-state";

        private CxRiskState()
        { }

        [JsonObject(MemberSerialization.OptIn)]
        public class RiskState
        {
            internal static RiskState Default(String projectId, String packageId, String vulnerabilityId) => 
                new RiskState() { ProjectId = projectId,  PackageId = packageId, VulnerabilityId = vulnerabilityId, State = "ToVerify"};

            [JsonProperty(PropertyName = "projectId")]
            public String ProjectId { get; internal set; }
            [JsonProperty(PropertyName = "packageId")]
            public String PackageId { get; internal set; }
            [JsonProperty(PropertyName = "vulnerabilityId")]
            public String VulnerabilityId { get; internal set; }
            [JsonProperty(PropertyName = "state")]
            public String State { get; internal set; }
        }

        [JsonArray]
        public class IndexedRiskStates : AggregatedCollection<RiskState>
        {
            public String ProjectId { get; internal set; }

            private Dictionary<String, RiskState> _index = new();
            public RiskState Lookup(String packageId, String vulnerabilityId)
            {
                if (!_index.ContainsKey(makeKey(packageId, vulnerabilityId)))
                    return RiskState.Default(ProjectId, packageId, vulnerabilityId);
                else
                    return _index[makeKey(packageId, vulnerabilityId)];
            }

            private String makeKey(String packageId, String vulnerabilityId) => $"{packageId}-{vulnerabilityId}";

            public override void Add(RiskState item)
            {
                base.Add(item);
                _index[makeKey(item.PackageId, item.VulnerabilityId)] = item;
            }
        }

        public static IndexedRiskStates GetRiskStates(CxSCARestContext ctx, CancellationToken token, String projectId)
        {
            var idx = WebOperation.ExecuteGet<IndexedRiskStates>(ctx.Json.CreateClient,
                (response) =>
                {
                    return JsonUtils.DeserializeFromStream<IndexedRiskStates>(response.Content.ReadAsStream());
                },
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX, projectId), ctx, token);

            idx.ProjectId = projectId;

            return idx;

        }

    }
}
