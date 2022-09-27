using CxAnalytix.Exceptions;
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

namespace CxRestClient.CXONE
{
    public class CxScanResults
    {
        private static ILog _log = LogManager.GetLogger(typeof(CxScanResults));

        private static String URL_SUFFIX = "api/results";


        [JsonObject(MemberSerialization.OptIn)]
        public class ScaResult
        {

        }

        [JsonObject(MemberSerialization.OptIn)]
        public class SastResult
        {

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

    }
}
