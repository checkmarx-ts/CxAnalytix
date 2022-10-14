using CxAnalytix.Exceptions;
using CxRestClient.SCA;
using CxRestClient.Utility;
using log4net;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static CxRestClient.SCA.CxDetailedReport;
using static CxRestClient.SCA.CxRiskState;

namespace CxRestClient.CXONE
{
    public static class CxScanResults
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxScanResults));

        private static String URL_SUFFIX = "api/results";


        [JsonObject(MemberSerialization.OptIn)]
        public class CommonResultElements<T>
        {

            [JsonProperty(PropertyName = "id")]
            public String ResultId { get; internal set; }

            [JsonProperty(PropertyName = "similarityId")]
            public String SimilarityId { get; internal set; }

            [JsonProperty(PropertyName = "status")]
            public String Status { get; internal set; }

            [JsonProperty(PropertyName = "state")]
            public String State { get; internal set; }

            [JsonProperty(PropertyName = "severity")]
            public String ResultSeverity { get; internal set; }

            [JsonProperty(PropertyName = "created")]
            public DateTime Created { get; internal set; }

            [JsonProperty(PropertyName = "firstFoundAt")]
            public DateTime FirstFoundDate { get; internal set; }

            [JsonProperty(PropertyName = "data")]
            public T Data { get; internal set; }


        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ScaExploitableMethods
        {
            [JsonProperty(PropertyName = "fullName")]
            public String FullName { get; internal set; }

            [JsonProperty(PropertyName = "namespace")]
            public String Namespace { get; internal set; }

            [JsonProperty(PropertyName = "shortName")]
            public String ShortName { get; internal set; }

            [JsonProperty(PropertyName = "sourceFile")]
            public String SourceFile { get; internal set; }

            [JsonProperty(PropertyName = "packageUiIdentifier")]
            public String PackageIdentifier { get; internal set; }
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class ScaResultData
        {
            [JsonProperty(PropertyName = "packageIdentifier")]
            public String PackageIdentifier { get; internal set; }

            [JsonProperty(PropertyName = "publishedAt")]
            public DateTime PublishedAt { get; internal set; }

            [JsonProperty(PropertyName = "recommendations")]
            public String Recommendations { get; internal set; }

            [JsonProperty(PropertyName = "recommendedVersion")]
            public String RecommendedVersion { get; internal set; }

            [JsonProperty(PropertyName = "exploitableMethods")]
            public List<ScaExploitableMethods> ExploitableMethods { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ScaVulnerabilityDetails
        {
            [JsonProperty(PropertyName = "cvssScore")]
            public Double Score { get; internal set; }

            [JsonProperty(PropertyName = "cveName")]
            public String CveName { get; internal set; }

            [JsonProperty(PropertyName = "cweId")]
            public String CweId { get; internal set; }

            [JsonObject(MemberSerialization.OptIn)]
            public class Cvss
            {
                [JsonProperty(PropertyName = "version")]
                public String Version { get; internal set; }

                [JsonProperty(PropertyName = "attackVector")]
                public String AttackVector { get; internal set; }

                [JsonProperty(PropertyName = "availability")]
                public String Availability { get; internal set; }

                [JsonProperty(PropertyName = "confidentiality")]
                public String Confidentiality { get; internal set; }

                [JsonProperty(PropertyName = "attackComplexity")]
                public String AttackComplexity { get; internal set; }
            }

            [JsonProperty(PropertyName = "cvss")]
            public Cvss CvssDetails { get; internal set; }
        }

        [JsonObject(MemberSerialization.OptIn)]
        public class ScaResult : CommonResultElements<ScaResultData>
        {
            // TODO: SCA comments are not yet implemented.

            [JsonProperty(PropertyName = "vulnerabilityDetails")]
            public ScaVulnerabilityDetails VulnerabilityDetails { get; internal set; }

        }


        [JsonObject(MemberSerialization.OptIn)]
        public class SastResultData
        {
            [JsonProperty(PropertyName = "queryId")]
            public String QueryId { get; internal set; }

            [JsonProperty(PropertyName = "queryName")]
            public String QueryName { get; internal set; }

            [JsonProperty(PropertyName = "group")]
            public String QueryGroup { get; internal set; }

            [JsonProperty(PropertyName = "resultHash")]
            public String ResultHash { get; internal set; }

            [JsonProperty(PropertyName = "languageName")]
            public String LanguageName { get; internal set; }

            [JsonProperty(PropertyName = "nodes")]
            public List<SastDataFlowNode> Flow { get; internal set; }

        }

        [JsonObject(MemberSerialization.OptIn)]
        public class SastDataFlowNode
        {
            [JsonProperty(PropertyName = "id")]
            public String NodeUniqueId { get; internal set; }

            [JsonProperty(PropertyName = "Line")]
            public String NodeLine { get; internal set; }

            [JsonProperty(PropertyName = "name")]
            public String NodeShortName { get; internal set; }

            [JsonProperty(PropertyName = "column")]
            public String NodeColumn { get; internal set; }

            [JsonProperty(PropertyName = "length")]
            public String NodeLength { get; internal set; }

            [JsonProperty(PropertyName = "method")]
            public String NodeMethod { get; internal set; }

            [JsonProperty(PropertyName = "fileName")]
            public String NodeFileName { get; internal set; }

            [JsonProperty(PropertyName = "fullName")]
            public String NodeFullName { get; internal set; }

            [JsonProperty(PropertyName = "typeName")]
            public String NodeType { get; internal set; }

            [JsonProperty(PropertyName = "methodLine")]
            public String NodeMethodLine { get; internal set; }
        }


        [JsonObject(MemberSerialization.OptIn)]
        public class SastVulnerabilityDetails
        {
            [JsonProperty(PropertyName = "cweId")]
            public String CweId { get; internal set; }

            [JsonProperty(PropertyName = "compliances")]
            public List<String> Categories { get; internal set; }
        }



        [JsonObject(MemberSerialization.OptIn)]
        public class SastResult : CommonResultElements<SastResultData>
        {

            [JsonProperty(PropertyName = "vulnerabilityDetails")]
            public SastVulnerabilityDetails VulnerabilityDetails { get; internal set; }

            // TODO: Comments don't work in this API as of this time.  Comments are only found in the sast-predicates API, which
            // would require making a web service call for each simid.
        }



        public class ResultsCollection
        {
            public void Add(ResultsCollection other)
            {
                SastResults.AddRange(other.SastResults);
                ScaResults.AddRange(other.ScaResults);
            }

            public List<SastResult> SastResults { get; internal set; } = new();

            public List<ScaResult> ScaResults { get; internal set; } = new();
        }


        private static readonly int MAX_RESULTS_PER_PAGE = 10000;

        public static async Task<ResultsCollection> GetScanResults(CxOneRestContext ctx, CancellationToken token, String scanId)
        {
            ResultsCollection response = new();

            Dictionary<String, String> parameters = new() { { "scan-id", scanId } };

            var serializer = new JsonSerializer();


            await PageableOperation.DoPagedGetRequest<ResultsCollection>((rc) =>
            {
                response.Add(rc);

                return rc.ScaResults.Count + rc.SastResults.Count;

            }, ctx.Json.CreateClient, 

            (responseMsg) => {

                ResultsCollection deserialized = new();

                using (var textReader = new StreamReader(responseMsg.Content.ReadAsStream(token), Encoding.UTF8))
                using (var jsonReader = new JsonTextReader(textReader))
                {
                    while (jsonReader.TokenType != JsonToken.StartArray)
                        jsonReader.Read();

                    var serializedArray = JToken.Load(jsonReader) as JArray;

                    foreach(var jtoken in serializedArray)
                    {
                        JToken typeToken = null;

                        foreach(var child in jtoken.Children<JProperty>())
                        {
                            if (child.Name.CompareTo("type") == 0)
                            {
                                typeToken = child;
                                break;
                            }
                        }

                        if (typeToken == null)
                            throw new UnrecoverableOperationException("Unable to locate \"type\" field in API response JSON.");

                        var typeValue = typeToken.ToObject<String>();

                        if (typeValue.CompareTo("sast") == 0)
                            using (var objectReader = jtoken.CreateReader())
                            {
                                deserialized.SastResults.Add((SastResult)serializer.Deserialize(objectReader, typeof(SastResult)));
                                continue;
                            }


                        if (typeValue.CompareTo("sca") == 0)
                            using (var objectReader = jtoken.CreateReader())
                                deserialized.ScaResults.Add((ScaResult)serializer.Deserialize(objectReader, typeof(ScaResult)));
                    }

                }

                return deserialized;
            
            },
                UrlUtils.MakeUrl(ctx.ApiUrl, URL_SUFFIX, parameters), ctx, token, pageSize: MAX_RESULTS_PER_PAGE);

            return response;
        }

        public static async Task<DetailedRiskReport> GetScaScanResults(CxOneRestContext ctx, CancellationToken token, String scanId)
        {
            return await Task.Run(() => CxDetailedReport.GetDetailedReport(ctx, token, scanId, "api/sca"));
        }

        public static async Task<IndexedRiskStates> GetScaRiskStates(CxOneRestContext ctx, CancellationToken token, String projectId)
        {
            return await Task.Run(() => CxRiskState.GetRiskStates(ctx, token, projectId, "api/sca"));
        }

    }
}
