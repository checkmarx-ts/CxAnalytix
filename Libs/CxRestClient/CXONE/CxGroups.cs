using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static CxRestClient.CXONE.CxProjects;

namespace CxRestClient.CXONE
{
    public static class CxGroups
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxGroups));

        private static String URL_RIGHT_SUFFIX = "groups?briefRepresentation=true";
        private static String URL_LEFT_SUFFIX = "auth/admin/realms";


        [JsonObject(MemberSerialization.OptIn)]
        public class Group
        {
            public Group(Group right)
            {
                if (right != null)
                {
                    Id = right.Id;
                    Name = right.Name;
                    Path = right.Path;
                }
            }

            [JsonProperty(PropertyName = "id")]
            public String Id { get; internal set; }

            [JsonProperty(PropertyName = "name")]
            public String Name { get; internal set; }

            [JsonProperty(PropertyName = "path")]
            public String Path { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        internal class NestedGroup : Group
        {
            public NestedGroup(NestedGroup right) : base(right)
            {
                if (right != null)
                    Subgroups = right.Subgroups;
            }

            [JsonProperty(PropertyName = "subGroups")]
            public List<NestedGroup> Subgroups { get; internal set; }
        }

        private static void RecurseIntoAndAdd(NestedGroup response, ConcurrentDictionary<String, Group> dest)
        {
            if (response == null)
                return;

            dest.TryAdd(response.Id, new Group(response));

            if (response.Subgroups != null && response.Subgroups.Count > 0)
                Parallel.ForEach(response.Subgroups, (rootGroup) => RecurseIntoAndAdd(rootGroup, dest));
        }

        private static IDictionary<String, Group> RecursiveIndex(List<NestedGroup> response, CancellationToken token)
        {
            ConcurrentDictionary<String, Group> result = new();

            Parallel.ForEach(response, new ParallelOptions() { CancellationToken = token }, (rootGroup) => RecurseIntoAndAdd(rootGroup, result));

            return result;
        }

        public static Task<IDictionary<String, Group>> GetGroups(CxOneRestContext ctx, CancellationToken token)
        {
            return Task.Run(() => RecursiveIndex(WebOperation.ExecuteGet<List<NestedGroup>>
                (ctx.Json.CreateClient, JsonUtils.DeserializeResponse<List<NestedGroup>>,
                UrlUtils.MakeUrl(ctx.IAMUrl, URL_LEFT_SUFFIX, ctx.Tenant, URL_RIGHT_SUFFIX), ctx, token), token));
        }
    }
}
