using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;

namespace CxRestClient
{
    public class CxScaVulnerabilities
    {
        private static String URL_SUFFIX = "cxrestapi/osa/vulnerabilities";


        private CxScaVulnerabilities()
        { }

        public class Severity
        {
            [JsonProperty(PropertyName = "name")]
            public String Name { get; internal set; }
        }

        public class State
        {
            [JsonProperty(PropertyName = "actionType")]
            public String ActionType { get; internal set; }
            [JsonProperty(PropertyName = "name")]
            public String StateName { get; internal set; }

        }

        public class Vulnerability
        {
            [JsonProperty(PropertyName = "id")]
            public String VulerabilityId { get; internal set; }
            [JsonProperty(PropertyName = "cveName")]
            public String CVEName { get; internal set; }
            [JsonProperty(PropertyName = "score")]
            public double CVEScore { get; internal set; }
            [JsonProperty(PropertyName = "severity")]
            public Severity Severity { get; internal set; }
            [JsonProperty(PropertyName = "publishDate")]
            public String CVEPublishDate { get; internal set; }
            [JsonProperty(PropertyName = "url")]
            public String CVEUrl { get; internal set; }
            [JsonProperty(PropertyName = "description")]
            public String CVEDescription { get; internal set; }
            [JsonProperty(PropertyName = "recommendations")]
            public String Recommendations { get; internal set; }
            [JsonProperty(PropertyName = "sourceFileName")]
            public String SourceFilename { get; internal set; }
            [JsonProperty(PropertyName = "libraryId")]
            public String LibraryId { get; internal set; }
            [JsonProperty(PropertyName = "state")]
            public State State { get; internal set; }
            [JsonProperty(PropertyName = "similarityId")]
            public String SimilarityId { get; internal set; }
        }


        private class VulnerabilityReader : IEnumerable<Vulnerability>, IEnumerator<Vulnerability>
        {
            private JToken _json;
            private JTokenReader _reader;

            internal VulnerabilityReader(JToken json)
            {
                _json = json;
                _reader = new JTokenReader(_json);
            }

            private Vulnerability _currentVuln;


            public Vulnerability Current => _currentVuln;

            object IEnumerator.Current => Current;

            public IEnumerator<Vulnerability> GetEnumerator()
            {
                return new VulnerabilityReader(_json);
            }

            void IDisposable.Dispose()
            {
                _reader = null;
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            int _arrayPos = 0;
            JArray _vulnArray;

            public bool MoveNext()
            {
                if (_reader.CurrentToken == null)
                {
                    while (_reader.Read() && _reader.CurrentToken.Type != JTokenType.Array) ;
                    if (_reader.CurrentToken == null || _reader.CurrentToken.Type != JTokenType.Array)
                        return false;

                    _vulnArray = (JArray)_reader.CurrentToken;
                }
                else
                    _arrayPos++;

                if (!(_arrayPos < _vulnArray.Count))
                    return false;

                _currentVuln = (Vulnerability)new JsonSerializer().
                    Deserialize(new JTokenReader(_vulnArray[_arrayPos]), typeof(Vulnerability));

                return true;
            }

            void IEnumerator.Reset()
            {
                throw new NotImplementedException();
            }
        }


        public static IEnumerable<Vulnerability> GetVulnerabilities
            (CxRestContext ctx, CancellationToken token, String scanId)
        {
            String url = CxRestContext.MakeUrl(ctx.Url, URL_SUFFIX, new Dictionary<String, String>()
            {
                {"scanId", Convert.ToString (scanId)  }
            });

            using (var client = ctx.Json.CreateSastClient())
            using (var vulns = client.GetAsync(url, token).Result)
            {

                if (token.IsCancellationRequested)
                    return null;

                if (!vulns.IsSuccessStatusCode)
                    throw new InvalidOperationException(vulns.ReasonPhrase);

                using (var sr = new StreamReader
                    (vulns.Content.ReadAsStreamAsync().Result))
                using (var jtr = new JsonTextReader(sr))
                {
                    JToken jt = JToken.Load(jtr);
                    return new VulnerabilityReader(jt);
                }
            }
        }

    }
}
