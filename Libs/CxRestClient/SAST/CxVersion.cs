using CxAnalytix.Exceptions;
using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace CxRestClient.SAST
{
    public class CxVersion
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxVersion));

        private static String URL_SUFFIX = "cxrestapi/system/version";

        private static Regex _versionMatcher = new(@"^(?<major>\d+)\.(?<minor>\d+)\..*$");

        [JsonObject(MemberSerialization.OptIn)]
        public class ServerVersion
        {
            [JsonProperty(PropertyName = "version")]
            public String Version { get; internal set; }
            [JsonProperty(PropertyName = "hotFix")]
            public String HotFix { get; internal set; }

            [JsonProperty(PropertyName = "enginePackVersion")]
            public String EnginePack { get; internal set; }
        }


        public class MajorMinor
        {
            public MajorMinor()
            {
                IsUnknown = true;
            }
            public MajorMinor(int major, int minor, int hf)
            {
                IsUnknown = false;
                Major = major;
                Minor = minor;
                Hotfix = hf;
            }

            public int Major { get; internal set; }
            public int Minor { get; internal set; }
            public int Hotfix { get; internal set; }

            public bool IsUnknown { get; internal set; }
        }

        public static ServerVersion GetServerVersion(CxSASTRestContext ctx, CancellationToken token)
        {
            var requestUrl = UrlUtils.MakeUrl(ctx.Sast.ApiUrl, URL_SUFFIX);
            var apiVersion = "1.1";

            return WebOperation.ExecuteGet<ServerVersion>(
            ctx.Sast.Json.CreateClient
            , (response) =>
            {
                using (var sr = new StreamReader(response.Content.ReadAsStreamAsync().Result))
                using (var stream = new JsonTextReader(sr))
                {
                    JToken jt = JToken.Load(stream);

                    using (var jtr = new JTokenReader(jt))
                        return (ServerVersion)new JsonSerializer().Deserialize(jtr, typeof(ServerVersion));
                }
            }
            , requestUrl
            , ctx.Sast
            , token, apiVersion: apiVersion,
            responseErrorLogic: (err) => {
                throw new UnsupportedAPIException(requestUrl, apiVersion);
            } );
        }

        public static MajorMinor GetServerMajorMinorVersion(CxSASTRestContext ctx, CancellationToken token)
        {
            try
            {
                var v = GetServerVersion(ctx, token);

                var m = _versionMatcher.Match(v.Version);
                
                if (m.Success)
                    return new MajorMinor(Convert.ToInt32(m.Groups["major"].Value), Convert.ToInt32(m.Groups["minor"].Value), Convert.ToInt32(v.HotFix));
            }
            catch (UnsupportedAPIException)
            {
                // Some versions of SAST may not have this API.
            }

            return new MajorMinor();
        }
    }
}
