using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.CXONE
{
    public static class CxConfiguration
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxConfiguration));
        private static String URL_SUFFIX = "api/configuration";

        [JsonObject(MemberSerialization.OptIn)]
        public class ConfigEntry
        {
            [JsonProperty(PropertyName = "key")]
            public String ConfigKey { get; internal set; }

            [JsonProperty(PropertyName = "name")]
            public String ConfigName { get; internal set; }

            [JsonProperty(PropertyName = "category")]
            public String ConfigCategory { get; internal set; }

            [JsonProperty(PropertyName = "originLevel")]
            public String ConfigOriginLevel { get; internal set; }

            [JsonProperty(PropertyName = "value")]
            public String ConfigValue { get; internal set; }

            [JsonProperty(PropertyName = "valuetype")]
            public String ConfigValueType { get; internal set; }

            [JsonProperty(PropertyName = "allowOverride")]
            public bool AllowOverride { get; internal set; }
        }

        public class ProjectConfigurationCollection : SingleIndexedCollection<ConfigEntry, String>
        {
            public override string GetIndexKey(ConfigEntry item) => item.ConfigKey;
        }

        public class ProjectConfiguration : ProjectConfigurationCollection
        {

            public String SastFileFilter => this["scan.config.sast.filter"].ConfigValue;
            public String Preset => this["scan.config.sast.presetName"].ConfigValue;

        }

        public static async Task<ProjectConfiguration> GetProjectConfiguration(CxOneRestContext ctx, CancellationToken token, String projectId)
        {
            return await WebOperation.ExecuteGetAsync(ctx.Json.CreateClient,
                JsonUtils.DeserializeResponse<ProjectConfiguration>, 
                UrlUtils.MakeUrl (UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX, "project"), 
                new Dictionary<String, String>() { { "project-id", projectId} }), ctx, token);
        }


    }
}
