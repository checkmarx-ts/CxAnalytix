using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using static CxRestClient.CXONE.CxScanResults;

namespace CxRestClient.CXONE
{
    public static class CxAudit
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxAudit));

        private static String URL_SUFFIX = "api/cx-audit/queries";

        public enum Severity
        {
            INFO = 0,
            LOW = 1,
            MEDIUM = 2,
            HIGH = 3
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class QueryBase
        {
            [JsonProperty(PropertyName = "Id")]
            public String Id { get; internal set; }

            [JsonProperty(PropertyName = "level")]
            public String Level { get; internal set; }

            [JsonProperty(PropertyName = "path")]
            public String Path { get; internal set; }

            [JsonProperty(PropertyName = "isExecutable")]
            public bool Executable { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class Query : QueryBase
        {
            [JsonProperty(PropertyName = "name")]
            public String Name { get; internal set; }

            [JsonProperty(PropertyName = "group")]
            public String Group { get; internal set; }

            [JsonProperty(PropertyName = "lang")]
            public String Language { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class QuerySource : QueryBase
        {
            [JsonProperty(PropertyName = "Modified")]
            public DateTime Modified { get; internal set; }

            [JsonProperty(PropertyName = "Cwe")]
            public UInt32 Cwe { get; internal set; }

            [JsonProperty(PropertyName = "Severity")]
            public Severity Severity { get; internal set; }
        }

        public class QueriesIndex : SingleIndexedCollection<Query, String>
        {
            public override String GetIndexKey(Query item) => item.AsCompositeKey();
        }

        public class QuerySourceIndex : SingleIndexedCollection<QuerySource, String>
        {
            public override String GetIndexKey(QuerySource item) => item.AsCompositeKey();
        }

        private static String AsCompositeKey(this SastResultData data) => String.Join("/", data.LanguageName, data.QueryGroup, data.QueryName);
        private static String AsCompositeKey(this Query data) => String.Join("/", data.Language, data.Group, data.Name);
        private static String AsCompositeKey(this QuerySource data) => data.Path;
        private static String AsSourceReferenceKey(this Query data) => data.Path;


        public class QueryDataMediator
        {
            private CxOneRestContext _ctx;
            private CancellationToken _token;

            private Dictionary<String, QueriesIndex> _projectQueries = new();
            private Dictionary<String, QuerySourceIndex> _projectQuerySources = new();

            public QueryDataMediator(CxOneRestContext ctx, CancellationToken token)
            {
                _ctx = ctx;
                _token = token;
            }

            public QueriesIndex GetQueryIndex(String projectId)
            {
                lock (_projectQueries)
                    if (!_projectQueries.ContainsKey(projectId))
                        using (var fetch = GetQueries(_ctx, _token, projectId))
                            _projectQueries.Add(projectId, fetch.Result);

                return _projectQueries[projectId];
            }

            public Query GetQuery(String projectId, SastResultData data) => GetQueryIndex(projectId)[data.AsCompositeKey()];

            public QuerySource GetQuerySource(String projectId, SastResultData data)
            {
                var query = GetQuery(projectId, data);

                lock (_projectQuerySources)
                {
                    if (!_projectQuerySources.ContainsKey(projectId))
                        _projectQuerySources.Add(projectId, new());

                    if (!_projectQuerySources[projectId].ContainsKey(query.Path))
                        using (var fetch = CxAudit.GetQuerySource(_ctx, _token, projectId, query.AsSourceReferenceKey()))
                            _projectQuerySources[projectId].Add(fetch.Result);
                }

                return _projectQuerySources[projectId][query.AsSourceReferenceKey()];
            } 
        }

        public static QueryDataMediator CreateQueryDataMediator(CxOneRestContext ctx, CancellationToken token) => new(ctx, token);

        public static async Task<QueriesIndex> GetQueries(CxOneRestContext ctx, CancellationToken token, String projectId)
        {
            return await WebOperation.ExecuteGetAsync<QueriesIndex>(ctx.Json.CreateClient, JsonUtils.DeserializeResponse<QueriesIndex>,
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX, new Dictionary<String, String>() { { "projectId", projectId} }), ctx, token);
        }

        public static async Task<QuerySource> GetQuerySource(CxOneRestContext ctx, CancellationToken token, String projectId, String path)
        {
            return await WebOperation.ExecuteGetAsync<QuerySource>(ctx.Json.CreateClient, JsonUtils.DeserializeResponse<QuerySource>,
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX, projectId, HttpUtility.UrlEncode(path) ), ctx, token);
        }
    }
}
