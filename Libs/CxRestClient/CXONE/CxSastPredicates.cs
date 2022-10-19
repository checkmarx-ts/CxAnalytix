using CxAnalytix.Extensions;
using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.CXONE
{
    public static class CxSastPredicates
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxSastPredicates));

        private static String URL_SUFFIX = "api/sast-results-predicates";


        [JsonObject(MemberSerialization.OptIn)]
        public class PredicateComment
        {
            [JsonProperty(PropertyName = "comment")]
            public String Comment { get; internal set; }

            [JsonProperty(PropertyName = "createdBy")]
            public String Commenter { get; internal set; }

            [JsonProperty(PropertyName = "createdAt")]
            public DateTime CommentDate { get; internal set; }

            public override string ToString()
            {
                return $"[{CommentDate}][{Commenter}][{Comment}]";
            }
        }

        public class ProjectPredicates
        {
            [JsonProperty(PropertyName = "projectId")]
            public String ProjectId { get; internal set; }

            [JsonProperty(PropertyName = "predicates")]
            public List<PredicateComment> Comments { get; internal set; }
        }

        public class PredicateProjectIndex : SingleIndexedCollection<ProjectPredicates, String>
        {
            public override string GetIndexKey(ProjectPredicates item) => item.ProjectId;
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class PredicateResponse
        {
            [JsonProperty(PropertyName = "predicateHistoryPerProject")]
            public PredicateProjectIndex Predicates { get; internal set; }

        }

        public class PredicateMediator : IDisposable
        {
            private CxOneRestContext _ctx;
            private CancellationToken _token;

            private long _hit = 0;
            private long _miss = 0;

            private ConcurrentDictionary<String, PredicateResponse> _loaded = new();

            public PredicateMediator(CxOneRestContext ctx, CancellationToken token)
            {
                _ctx = ctx;
                _token = token;
            }

            public List<String> GetComments(String projectId, String similarityId)
            {
                List<String> ret_val = new();

                lock(_loaded)
                {
                    if (!_loaded.ContainsKey(similarityId))
                    {
                        _loaded.TryAdd(similarityId, GetVulnerabilityPredicates(_ctx, _token, similarityId));
                        _miss++;

                    }
                    else
                        _hit++;
                }

                if (_loaded[similarityId].Predicates.ContainsKey(projectId))
                    foreach (var comment in _loaded[similarityId].Predicates[projectId].Comments)
                        ret_val.Add(comment.ToString());

                return ret_val;
            }

            public void Dispose()
            {
                _log.Debug($"PredicateMediator: {_hit} cache hits and {_miss} misses.");
            }
        }


        public static PredicateResponse GetVulnerabilityPredicates(CxOneRestContext ctx, CancellationToken token, String similarityId)
        {
            return WebOperation.ExecuteGet(ctx.Json.CreateClient, JsonUtils.DeserializeResponse<PredicateResponse>,
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX, similarityId), ctx, token);
        }

    }
}
